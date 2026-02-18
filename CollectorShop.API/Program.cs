using CollectorShop.API;
using CollectorShop.API.Middleware;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using CollectorShop.Infrastructure;
using Microsoft.AspNetCore.Identity;
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

    // Seed a Customer profile for the DevAuth user so cart/wishlist work in dev mode
    var bypassAuth = app.Configuration.GetValue<bool>("BypassAuth");
    if (bypassAuth)
    {
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
