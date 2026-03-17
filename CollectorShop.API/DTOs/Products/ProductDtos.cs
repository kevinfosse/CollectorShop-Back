using CollectorShop.Domain.Enums;

namespace CollectorShop.API.DTOs.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? CompareAtPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public int StockQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public ProductCondition Condition { get; set; }
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductAttributeDto> Attributes { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? CompareAtPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public ProductCondition Condition { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public double AverageRating { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public ProductCondition Condition { get; set; } = ProductCondition.New;
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public bool IsFeatured { get; set; }
    public List<CreateProductImageRequest> Images { get; set; } = new();
    public List<CreateProductAttributeRequest> Attributes { get; set; } = new();
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public ProductCondition Condition { get; set; }
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateProductImageRequest
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class ProductAttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class CreateProductAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ProductFilterRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductCondition? Condition { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
