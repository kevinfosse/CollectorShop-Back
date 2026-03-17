namespace CollectorShop.Infrastructure.Data.Seeding.Models;

public class ProductSeedDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EUR";
    public decimal? CompareAtPrice { get; set; }
    public string? CompareAtPriceCurrency { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int Condition { get; set; }
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public string CategoryId { get; set; } = null!;
    public string? BrandId { get; set; }
    public List<ImageSeedDto> Images { get; set; } = [];
    public List<AttributeSeedDto> Attributes { get; set; } = [];
}

public class ImageSeedDto
{
    public string Id { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class AttributeSeedDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}
