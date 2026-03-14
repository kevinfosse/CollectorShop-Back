using System.Reflection;
using System.Text.Json;
using CollectorShop.Infrastructure.Data.Seeding.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CollectorShop.Infrastructure.Data.Seeding;

public static class SeedDataHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly DateTime SeedDate = new(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc);

    public static void SeedCatalogData(MigrationBuilder migrationBuilder)
    {
        var categories = ReadEmbeddedJson<List<CategorySeedDto>>("categories.json");
        var brands = ReadEmbeddedJson<List<BrandSeedDto>>("brands.json");
        var products = ReadEmbeddedJson<List<ProductSeedDto>>("products.json");

        SeedCategories(migrationBuilder, categories);
        SeedBrands(migrationBuilder, brands);
        SeedProducts(migrationBuilder, products);
    }

    public static void UnseedCatalogData(MigrationBuilder migrationBuilder)
    {
        var products = ReadEmbeddedJson<List<ProductSeedDto>>("products.json");
        var brands = ReadEmbeddedJson<List<BrandSeedDto>>("brands.json");
        var categories = ReadEmbeddedJson<List<CategorySeedDto>>("categories.json");

        foreach (var p in products)
        {
            var productId = Guid.Parse(p.Id);

            foreach (var attr in p.Attributes)
                migrationBuilder.DeleteData("ProductAttributes", "Id", Guid.Parse(attr.Id));

            foreach (var img in p.Images)
                migrationBuilder.DeleteData("ProductImages", "Id", Guid.Parse(img.Id));

            migrationBuilder.DeleteData("Products", "Id", productId);
        }

        foreach (var b in brands)
            migrationBuilder.DeleteData("Brands", "Id", Guid.Parse(b.Id));

        foreach (var c in categories)
            migrationBuilder.DeleteData("Categories", "Id", Guid.Parse(c.Id));
    }

    private static T ReadEmbeddedJson<T>(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith($"SeedData.{fileName}", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    private static void SeedCategories(MigrationBuilder migrationBuilder, List<CategorySeedDto> categories)
    {
        foreach (var c in categories)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: ["Id", "Name", "Description", "Slug", "ImageUrl", "DisplayOrder", "IsActive", "ParentCategoryId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DeletedAt", "DeletedBy"],
                values: new object?[]
                {
                    Guid.Parse(c.Id), c.Name, c.Description, c.Slug, c.ImageUrl,
                    c.DisplayOrder, c.IsActive, c.ParentCategoryId != null ? Guid.Parse(c.ParentCategoryId) : null,
                    SeedDate, null, null, null, false, null, null
                });
        }
    }

    private static void SeedBrands(MigrationBuilder migrationBuilder, List<BrandSeedDto> brands)
    {
        foreach (var b in brands)
        {
            migrationBuilder.InsertData(
                table: "Brands",
                columns: ["Id", "Name", "Description", "Slug", "LogoUrl", "WebsiteUrl", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DeletedAt", "DeletedBy"],
                values: new object?[]
                {
                    Guid.Parse(b.Id), b.Name, b.Description, b.Slug, b.LogoUrl, b.WebsiteUrl, b.IsActive,
                    SeedDate, null, null, null, false, null, null
                });
        }
    }

    private static void SeedProducts(MigrationBuilder migrationBuilder, List<ProductSeedDto> products)
    {
        foreach (var p in products)
        {
            var productId = Guid.Parse(p.Id);

            migrationBuilder.InsertData(
                table: "Products",
                columns: ["Id", "Name", "Description", "Sku", "Price", "PriceCurrency", "CompareAtPrice", "CompareAtPriceCurrency", "StockQuantity", "ReservedQuantity", "IsActive", "IsFeatured", "Condition", "Weight", "Dimensions", "CategoryId", "BrandId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DeletedAt", "DeletedBy"],
                values: new object?[]
                {
                    productId, p.Name, p.Description, p.Sku,
                    p.Price, p.Currency, p.CompareAtPrice, p.CompareAtPriceCurrency,
                    p.StockQuantity, 0, p.IsActive, p.IsFeatured,
                    p.Condition, p.Weight, p.Dimensions,
                    Guid.Parse(p.CategoryId), p.BrandId != null ? Guid.Parse(p.BrandId) : null,
                    SeedDate, null, null, null, false, null, null
                });

            foreach (var img in p.Images)
            {
                migrationBuilder.InsertData(
                    table: "ProductImages",
                    columns: ["Id", "Url", "AltText", "DisplayOrder", "IsPrimary", "ProductId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DeletedAt", "DeletedBy"],
                    values: new object?[]
                    {
                        Guid.Parse(img.Id), img.Url, img.AltText, img.DisplayOrder, img.IsPrimary, productId,
                        SeedDate, null, null, null, false, null, null
                    });
            }

            foreach (var attr in p.Attributes)
            {
                migrationBuilder.InsertData(
                    table: "ProductAttributes",
                    columns: ["Id", "Name", "Value", "ProductId", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "DeletedAt", "DeletedBy"],
                    values: new object?[]
                    {
                        Guid.Parse(attr.Id), attr.Name, attr.Value, productId,
                        SeedDate, null, null, null, false, null, null
                    });
            }
        }
    }
}
