using CollectorShop.API.DTOs.Common;
using CollectorShop.API.DTOs.Products;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductListDto>>> GetProducts([FromQuery] ProductFilterRequest filter)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.SearchTerm,
            filter.CategoryId,
            filter.BrandId,
            filter.MinPrice,
            filter.MaxPrice,
            filter.Condition,
            filter.SortBy,
            filter.SortDescending
        );

        var productDtos = products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            CompareAtPrice = p.CompareAtPrice?.Amount,
            DiscountPercentage = CalculateDiscountPercentage(p.Price.Amount, p.CompareAtPrice?.Amount),
            AvailableQuantity = p.AvailableQuantity,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            Condition = p.Condition,
            CategoryName = p.Category?.Name,
            BrandName = p.Brand?.Name,
            PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? p.Images.FirstOrDefault()?.Url
        });

        return Ok(new PagedResponse<ProductListDto>
        {
            Items = productDtos,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(id);
        var reviewCount = await _unitOfWork.Reviews.CountAsync(r => r.ProductId == id && r.IsApproved);

        var dto = MapToProductDto(product, averageRating, reviewCount);
        return Ok(dto);
    }

    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetProductBySku(string sku)
    {
        var product = await _unitOfWork.Products.GetBySkuAsync(sku);
        if (product == null)
        {
            return NotFound();
        }

        var fullProduct = await _unitOfWork.Products.GetByIdWithDetailsAsync(product.Id);
        var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(product.Id);
        var reviewCount = await _unitOfWork.Reviews.CountAsync(r => r.ProductId == product.Id && r.IsApproved);

        var dto = MapToProductDto(fullProduct!, averageRating, reviewCount);
        return Ok(dto);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetFeaturedProducts([FromQuery] int count = 8)
    {
        var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);

        var productDtos = products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            CompareAtPrice = p.CompareAtPrice?.Amount,
            DiscountPercentage = CalculateDiscountPercentage(p.Price.Amount, p.CompareAtPrice?.Amount),
            AvailableQuantity = p.AvailableQuantity,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            Condition = p.Condition,
            PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? p.Images.FirstOrDefault()?.Url
        });

        return Ok(productDtos);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProductsByCategory(Guid categoryId)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);

        var productDtos = products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            CompareAtPrice = p.CompareAtPrice?.Amount,
            DiscountPercentage = CalculateDiscountPercentage(p.Price.Amount, p.CompareAtPrice?.Amount),
            AvailableQuantity = p.AvailableQuantity,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            Condition = p.Condition,
            PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? p.Images.FirstOrDefault()?.Url
        });

        return Ok(productDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Sku);
        if (existingProduct != null)
        {
            return BadRequest(new { Message = "A product with this SKU already exists" });
        }

        var product = new Product(
            request.Name,
            request.Description,
            request.Sku,
            new Money(request.Price, request.Currency),
            request.StockQuantity,
            request.CategoryId,
            request.Condition
        );

        if (request.BrandId.HasValue)
        {
            product.SetBrand(request.BrandId.Value);
        }

        if (request.CompareAtPrice.HasValue)
        {
            product.SetCompareAtPrice(new Money(request.CompareAtPrice.Value, request.Currency));
        }

        product.SetFeatured(request.IsFeatured);

        foreach (var imageRequest in request.Images)
        {
            var image = new ProductImage(product.Id, imageRequest.Url, imageRequest.AltText, imageRequest.DisplayOrder, imageRequest.IsPrimary);
            product.AddImage(image);
        }

        foreach (var attrRequest in request.Attributes)
        {
            var attribute = new ProductAttribute(product.Id, attrRequest.Name, attrRequest.Value);
            product.AddAttribute(attribute);
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductName} created with ID {ProductId}", product.Name, product.Id);

        var createdProduct = await _unitOfWork.Products.GetByIdWithDetailsAsync(product.Id);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToProductDto(createdProduct!, 0, 0));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.UpdateDetails(request.Name, request.Description, new Money(request.Price, request.Currency));
        product.SetCategory(request.CategoryId);
        product.SetBrand(request.BrandId);
        product.SetFeatured(request.IsFeatured);

        if (request.CompareAtPrice.HasValue)
        {
            product.SetCompareAtPrice(new Money(request.CompareAtPrice.Value, request.Currency));
        }
        else
        {
            product.SetCompareAtPrice(null);
        }

        if (request.IsActive)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        // Replace images
        product.ClearImages();
        foreach (var imageRequest in request.Images)
        {
            var image = new ProductImage(product.Id, imageRequest.Url, imageRequest.AltText, imageRequest.DisplayOrder, imageRequest.IsPrimary);
            product.AddImage(image);
        }

        // Replace attributes
        product.ClearAttributes();
        foreach (var attrRequest in request.Attributes)
        {
            var attribute = new ProductAttribute(product.Id, attrRequest.Name, attrRequest.Value);
            product.AddAttribute(attribute);
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} updated", id);

        var updatedProduct = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
        var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(id);
        var reviewCount = await _unitOfWork.Reviews.CountAsync(r => r.ProductId == id && r.IsApproved);

        return Ok(MapToProductDto(updatedProduct!, averageRating, reviewCount));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted", id);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (quantity > 0)
        {
            product.AddStock(quantity);
        }
        else if (quantity < 0)
        {
            product.RemoveStock(Math.Abs(quantity));
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { product.StockQuantity, product.AvailableQuantity });
    }

    private static int? CalculateDiscountPercentage(decimal price, decimal? compareAtPrice)
    {
        if (compareAtPrice == null || compareAtPrice <= price) return null;
        return (int)Math.Round((compareAtPrice.Value - price) / compareAtPrice.Value * 100);
    }

    private static ProductDto MapToProductDto(Product product, double averageRating, int reviewCount)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            CompareAtPrice = product.CompareAtPrice?.Amount,
            DiscountPercentage = CalculateDiscountPercentage(product.Price.Amount, product.CompareAtPrice?.Amount),
            StockQuantity = product.StockQuantity,
            AvailableQuantity = product.AvailableQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            Condition = product.Condition,
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            Images = product.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                DisplayOrder = i.DisplayOrder,
                IsPrimary = i.IsPrimary
            }).ToList(),
            Attributes = product.Attributes.Select(a => new ProductAttributeDto
            {
                Id = a.Id,
                Name = a.Name,
                Value = a.Value
            }).ToList(),
            AverageRating = averageRating,
            ReviewCount = reviewCount,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
