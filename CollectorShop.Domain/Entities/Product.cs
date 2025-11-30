using CollectorShop.Domain.Common;
using CollectorShop.Domain.Enums;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Sku { get; private set; }
    public Money Price { get; private set; }
    public Money? CompareAtPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public ProductCondition Condition { get; private set; }
    public decimal Weight { get; private set; }
    public string? Dimensions { get; private set; }
    
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    
    public Guid? BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<ProductAttribute> _attributes = new();
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private Product() { }

    public Product(
        string name,
        string description,
        string sku,
        Money price,
        int stockQuantity,
        Guid categoryId,
        ProductCondition condition = ProductCondition.New)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

        Name = name;
        Description = description ?? string.Empty;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        Condition = condition;
        IsActive = true;
        IsFeatured = false;
    }

    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    public void UpdateDetails(string name, string description, Money price)
    {
        Name = name;
        Description = description;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCompareAtPrice(Money? compareAtPrice)
    {
        CompareAtPrice = compareAtPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Insufficient available stock");
        
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Insufficient available stock to reserve");
        
        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (quantity > ReservedQuantity)
            throw new InvalidOperationException("Cannot release more than reserved quantity");
        
        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetFeatured(bool featured) => IsFeatured = featured;

    public void AddImage(ProductImage image)
    {
        _images.Add(image);
    }

    public void RemoveImage(ProductImage image)
    {
        _images.Remove(image);
    }

    public void AddAttribute(ProductAttribute attribute)
    {
        _attributes.Add(attribute);
    }

    public void SetCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBrand(Guid? brandId)
    {
        BrandId = brandId;
        UpdatedAt = DateTime.UtcNow;
    }
}
