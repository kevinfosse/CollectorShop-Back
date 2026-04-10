using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using CollectorShop.API.Authentication;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using CollectorShop.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace CollectorShop.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        // CORS - Environment-specific configuration
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                if (allowedOrigins.Length > 0)
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                }
                else
                {
                    // Fallback for development if not configured
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
            });
        });

        // Rate Limiting
        var rateLimitConfig = configuration.GetSection("RateLimiting");
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 100);
                limiterOptions.Window = TimeSpan.FromSeconds(rateLimitConfig.GetValue<int>("Window", 60));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = rateLimitConfig.GetValue<int>("QueueLimit", 2);
            });

            // Stricter limit for authentication endpoints
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        // Authentication
        var bypassAuth = configuration.GetValue<bool>("BypassAuth");

        if (bypassAuth)
        {
            // Development-only: bypass JWT, auto-authenticate as admin
            services.AddAuthentication(DevAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, null);
        }
        else
        {
            // JWT Authentication via Keycloak (OpenID Connect discovery)
            var keycloakSettings = configuration.GetSection("KeycloakSettings");
            var authority = keycloakSettings["Authority"] ?? throw new InvalidOperationException("KeycloakSettings:Authority is missing in configuration.");
            var audience = keycloakSettings["Audience"] ?? throw new InvalidOperationException("KeycloakSettings:Audience is missing in configuration.");
            var metadataAddress = keycloakSettings["MetadataAddress"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;

                if (!string.IsNullOrWhiteSpace(metadataAddress))
                {
                    options.MetadataAddress = metadataAddress;
                }

                options.RequireHttpsMetadata = false;

                // Allow internal pod-to-pod communication even with self-signed certs.
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "https://authcollector.duckdns.org/realms/CollectorShop",
                    ValidAudience = "collectorshop-client",
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Error("JWT authentication failed: {ErrorMessage}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        if (principal == null)
                        {
                            return;
                        }

                        var userId = principal.FindFirstValue("sub")
                                     ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
                        var email = principal.FindFirstValue("email")
                                    ?? principal.FindFirstValue(ClaimTypes.Email);
                        var firstName = principal.FindFirstValue("given_name")
                                        ?? principal.FindFirstValue(ClaimTypes.GivenName)
                                        ?? "Utilisateur";
                        var lastName = principal.FindFirstValue("family_name")
                                       ?? principal.FindFirstValue(ClaimTypes.Surname)
                                       ?? "CollectorShop";

                        var userName = principal.Identity?.Name ?? email ?? userId ?? "unknown";
                        Log.Information("JWT token validated successfully for user {UserName}", userName);

                        // Skip profile sync when required identity attributes are missing.
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            Log.Warning("JWT token validated without email claim, skipping local profile sync");
                            return;
                        }

                        var cancellationToken = context.HttpContext.RequestAborted;
                        using var scope = context.HttpContext.RequestServices.CreateScope();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        Customer? existingCustomer = null;
                        if (!string.IsNullOrWhiteSpace(userId))
                        {
                            existingCustomer = await unitOfWork.Customers.GetByUserIdAsync(userId, cancellationToken);
                        }

                        existingCustomer ??= await unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);

                        if (existingCustomer != null)
                        {
                            return;
                        }

                        var customer = new Customer(firstName, lastName, new Email(email), userId);
                        await unitOfWork.Customers.AddAsync(customer, cancellationToken);
                        await unitOfWork.SaveChangesAsync(cancellationToken);

                        Log.Information("Profil local créé pour l'utilisateur {Email}", email);
                    }
                };
            });
        }

        services.AddAuthorization();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CollectorShop API",
                Version = "v1",
                Description = "E-Commerce API for CollectorShop",
                Contact = new OpenApiContact
                {
                    Name = "CollectorShop Team"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        // Custom Services
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<CollectorShop.Domain.Interfaces.ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ShippingSettingsService>();

        return services;
    }
}
