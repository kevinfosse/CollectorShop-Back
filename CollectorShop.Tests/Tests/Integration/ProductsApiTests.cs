using System.Net;
using System.Net.Http.Json;
using CollectorShop.API.Authentication;
using CollectorShop.API.DTOs.Common;
using CollectorShop.API.DTOs.Products;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Enums;
using CollectorShop.Domain.ValueObjects;
using CollectorShop.Infrastructure.Data;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CollectorShop.Tests.Tests.Integration;

public class CollectorShopWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    public Guid SeededCategoryId { get; private set; }
    public Guid SeededProductId { get; private set; }
    public Guid ProductToDeleteId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BypassAuth"] = "true",
                ["ConnectionStrings:DefaultConnection"] = "placeholder",
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft"] = "Warning",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server DbContext with InMemory
            // Must remove the options config action too — otherwise both SqlServer
            // and InMemory providers coexist and EF Core throws at runtime.
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace MassTransit RabbitMQ with InMemory transport
            var massTransitServices = services
                .Where(d =>
                    d.ServiceType?.Namespace?.StartsWith("MassTransit") == true ||
                    d.ImplementationType?.Namespace?.StartsWith("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitServices)
                services.Remove(d);

            services.AddMassTransit(x => x.UsingInMemory());

            // Force DevAuthHandler as default scheme.
            // Must use PostConfigure: the app's AddApiServices already sets
            // DefaultAuthenticateScheme=JwtBearer via named Configure<AuthenticationOptions>,
            // so a plain AddAuthentication() call is not enough — PostConfigure runs last.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(
                    DevAuthHandler.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = DevAuthHandler.SchemeName;
                options.DefaultChallengeScheme = DevAuthHandler.SchemeName;
                options.DefaultScheme = DevAuthHandler.SchemeName;
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedTestData(host.Services);
        return host;
    }

    private void SeedTestData(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = new Category("Test Category", "Test Description", "test-category");
        db.Categories.Add(category);
        db.SaveChanges();
        SeededCategoryId = category.Id;

        var product = new Product(
            "Figurine Test",
            "Un produit de test",
            "TEST-FIGUR-001",
            new Money(29.99m, "EUR"),
            10,
            SeededCategoryId);

        var productToDelete = new Product(
            "Produit à Supprimer",
            "Ce produit sera supprimé",
            "TEST-DEL-001",
            new Money(9.99m, "EUR"),
            1,
            SeededCategoryId);

        db.Products.Add(product);
        db.Products.Add(productToDelete);
        db.SaveChanges();

        SeededProductId = product.Id;
        ProductToDeleteId = productToDelete.Id;
    }
}

[Trait("Category", "Integration")]
public class ProductsApiTests : IClassFixture<CollectorShopWebApplicationFactory>
{
    private readonly CollectorShopWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsApiTests(CollectorShopWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsOkWithAtLeastOneProduct()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductListDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProduct_WithSeededId_ReturnsOk()
    {
        var response = await _client.GetAsync($"/api/products/{_factory.SeededProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(_factory.SeededProductId);
    }

    [Fact]
    public async Task GetProduct_WithRandomGuid_Returns404()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_AsAdmin_Returns201()
    {
        var request = new CreateProductRequest
        {
            Name = "Nouveau Produit",
            Description = "Description du produit",
            Sku = "TEST-NEW-001",
            Price = 49.99m,
            Currency = "EUR",
            StockQuantity = 5,
            Condition = ProductCondition.New,
            CategoryId = _factory.SeededCategoryId,
        };

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Nouveau Produit");
        result.Sku.Should().Be("TEST-NEW-001");
    }

    [Fact]
    public async Task DeleteProduct_AsAdmin_Returns204()
    {
        var response = await _client.DeleteAsync($"/api/products/{_factory.ProductToDeleteId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
