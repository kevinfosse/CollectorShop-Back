using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string Url { get; private set; }
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    private ProductImage()
    {
        Url = null!;
    }

    public ProductImage(Guid productId, string url, string? altText = null, int displayOrder = 0, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty", nameof(url));

        ProductId = productId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetAsPrimary()
    {
        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAltText(string? altText)
    {
        AltText = altText;
        UpdatedAt = DateTime.UtcNow;
    }
}
