using CollectorShop.API.DTOs.Brands;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(IUnitOfWork unitOfWork, ILogger<BrandsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        var brands = await _unitOfWork.Brands.GetActiveBrandsAsync();

        var brandDtos = brands.Select(b => new BrandDto
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            Slug = b.Slug,
            LogoUrl = b.LogoUrl,
            WebsiteUrl = b.WebsiteUrl,
            IsActive = b.IsActive,
            ProductCount = b.Products.Count
        });

        return Ok(brandDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BrandDto>> GetBrand(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdWithProductsAsync(id);
        if (brand == null)
        {
            return NotFound();
        }

        return Ok(new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            Slug = brand.Slug,
            LogoUrl = brand.LogoUrl,
            WebsiteUrl = brand.WebsiteUrl,
            IsActive = brand.IsActive,
            ProductCount = brand.Products.Count
        });
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<BrandDto>> GetBrandBySlug(string slug)
    {
        var brand = await _unitOfWork.Brands.GetBySlugAsync(slug);
        if (brand == null)
        {
            return NotFound();
        }

        var fullBrand = await _unitOfWork.Brands.GetByIdWithProductsAsync(brand.Id);

        return Ok(new BrandDto
        {
            Id = fullBrand!.Id,
            Name = fullBrand.Name,
            Description = fullBrand.Description,
            Slug = fullBrand.Slug,
            LogoUrl = fullBrand.LogoUrl,
            WebsiteUrl = fullBrand.WebsiteUrl,
            IsActive = fullBrand.IsActive,
            ProductCount = fullBrand.Products.Count
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandRequest request)
    {
        var existingBrand = await _unitOfWork.Brands.GetBySlugAsync(request.Slug);
        if (existingBrand != null)
        {
            return BadRequest(new { Message = "A brand with this slug already exists" });
        }

        var brand = new Brand(request.Name, request.Description, request.Slug);

        if (!string.IsNullOrEmpty(request.LogoUrl))
        {
            brand.SetLogoUrl(request.LogoUrl);
        }

        if (!string.IsNullOrEmpty(request.WebsiteUrl))
        {
            brand.SetWebsiteUrl(request.WebsiteUrl);
        }

        await _unitOfWork.Brands.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandName} created with ID {BrandId}", brand.Name, brand.Id);

        return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            Slug = brand.Slug,
            LogoUrl = brand.LogoUrl,
            WebsiteUrl = brand.WebsiteUrl,
            IsActive = brand.IsActive,
            ProductCount = 0
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
        {
            return NotFound();
        }

        var existingWithSlug = await _unitOfWork.Brands.GetBySlugAsync(request.Slug);
        if (existingWithSlug != null && existingWithSlug.Id != id)
        {
            return BadRequest(new { Message = "A brand with this slug already exists" });
        }

        brand.UpdateDetails(request.Name, request.Description, request.Slug);
        brand.SetLogoUrl(request.LogoUrl);
        brand.SetWebsiteUrl(request.WebsiteUrl);

        if (request.IsActive)
        {
            brand.Activate();
        }
        else
        {
            brand.Deactivate();
        }

        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandId} updated", id);

        var updatedBrand = await _unitOfWork.Brands.GetByIdWithProductsAsync(id);

        return Ok(new BrandDto
        {
            Id = updatedBrand!.Id,
            Name = updatedBrand.Name,
            Description = updatedBrand.Description,
            Slug = updatedBrand.Slug,
            LogoUrl = updatedBrand.LogoUrl,
            WebsiteUrl = updatedBrand.WebsiteUrl,
            IsActive = updatedBrand.IsActive,
            ProductCount = updatedBrand.Products.Count
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdWithProductsAsync(id);
        if (brand == null)
        {
            return NotFound();
        }

        if (brand.Products.Any())
        {
            return BadRequest(new { Message = "Cannot delete brand with products. Remove products from brand first." });
        }

        _unitOfWork.Brands.Remove(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandId} deleted", id);

        return NoContent();
    }
}
