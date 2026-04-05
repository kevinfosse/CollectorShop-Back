using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Enums;
using CollectorShop.Domain.ValueObjects;
using FluentAssertions;

namespace CollectorShop.Tests.Tests.Unit;

public class ProductEntityTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Money DefaultPrice = new Money(29.99m, "EUR");

    private static Product CreateProduct(
        string name = "Figurine vintage",
        string sku = "SKU-001",
        int stock = 10)
        => new Product(name, "Une belle figurine", sku, DefaultPrice, stock, CategoryId);

    [Fact]
    public void Constructor_WithValidParameters_CreatesActiveProduct()
    {
        var product = CreateProduct("Figurine vintage", "SKU-001", 5);

        product.IsActive.Should().BeTrue();
        product.Name.Should().Be("Figurine vintage");
        product.Sku.Should().Be("SKU-001");
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => CreateProduct(name: "");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithNegativeStock_ThrowsArgumentException()
    {
        var act = () => CreateProduct(stock: -1);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("stockQuantity");
    }

    [Fact]
    public void AddStock_WithPositiveQuantity_IncreasesStock()
    {
        var product = CreateProduct(stock: 10);

        product.AddStock(5);

        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void AddStock_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = CreateProduct(stock: 10);

        var act = () => product.AddStock(0);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public void RemoveStock_MoreThanAvailable_ThrowsInvalidOperationException()
    {
        var product = CreateProduct(stock: 5);

        var act = () => product.RemoveStock(10);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient available stock*");
    }

    [Fact]
    public void ReserveStock_ReducesAvailableQuantity()
    {
        var product = CreateProduct(stock: 10);

        product.ReserveStock(3);

        product.AvailableQuantity.Should().Be(product.StockQuantity - product.ReservedQuantity);
        product.AvailableQuantity.Should().Be(7);
    }

    [Fact]
    public void UpdateDetails_ChangesNameDescriptionPrice()
    {
        var product = CreateProduct();
        var newPrice = new Money(49.99m, "EUR");

        product.UpdateDetails("Nouveau nom", "Nouvelle description", newPrice);

        product.Name.Should().Be("Nouveau nom");
        product.Description.Should().Be("Nouvelle description");
        product.Price.Amount.Should().Be(49.99m);
    }
}
