using CollectorShop.API;
using CollectorShop.API.Middleware;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using CollectorShop.Infrastructure;
using CollectorShop.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CollectorShop.Infrastructure.Data.ApplicationDbContext>("database");

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    Log.Information("Database migrations applied successfully");
}

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Customer", "Manager" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    // Seed admin user only when SeedAdmin config section is present (Development only)
    var seedAdminSection = app.Configuration.GetSection("SeedAdmin");
    if (seedAdminSection.Exists())
    {
        var adminEmail = seedAdminSection["Email"];
        var adminPassword = seedAdminSection["Password"];
        var adminFirstName = seedAdminSection["FirstName"] ?? "Admin";
        var adminLastName = seedAdminSection["LastName"] ?? "CollectorShop";

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CollectorShop.Infrastructure.Data.ApplicationUser>>();
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new CollectorShop.Infrastructure.Data.ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "Admin", "Customer", "Manager" });

                    var adminCustomer = new Customer(adminFirstName, adminLastName, new Email(adminEmail), adminUser.Id);
                    await unitOfWork.Customers.AddAsync(adminCustomer);
                    await unitOfWork.SaveChangesAsync();

                    Log.Information("Seeded admin user {Email} with all roles", adminEmail);
                }
                else
                {
                    Log.Warning("Failed to seed admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    // Seed a Customer profile for the DevAuth user so cart/wishlist work in dev mode
    var bypassAuth = app.Configuration.GetValue<bool>("BypassAuth");
    if (bypassAuth)
    {
        var existingCustomer = await unitOfWork.Customers.GetByUserIdAsync("dev-user-id");
        if (existingCustomer == null)
        {
            var devCustomer = new Customer(
                "Dev",
                "User",
                new Email("dev@collectorshop.local"),
                "dev-user-id"
            );
            await unitOfWork.Customers.AddAsync(devCustomer);
            await unitOfWork.SaveChangesAsync();
            Log.Information("Created dev Customer profile for DevAuth user");
        }
    }
}

// Exception handling should be first in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CollectorShop API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("DefaultPolicy");

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");
app.MapHealthChecks("/health");

app.Run();
