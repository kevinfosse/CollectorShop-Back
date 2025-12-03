using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Entities;

public class ProductAttribute : BaseEntity
{
    public string Name { get; private set; }
    public string Value { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    private ProductAttribute()
    {
        Name = null!;
        Value = null!;
    }

    public ProductAttribute(Guid productId, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be empty", nameof(name));

        ProductId = productId;
        Name = name;
        Value = value ?? string.Empty;
    }

    public void UpdateValue(string value)
    {
        Value = value ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
