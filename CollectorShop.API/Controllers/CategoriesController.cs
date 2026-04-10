using CollectorShop.API.DTOs.Categories;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IUnitOfWork unitOfWork, ILogger<CategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryListDto>>> GetCategories()
    {
        var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();

        var categoryDtos = categories.Select(c => new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            ImageUrl = c.ImageUrl,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ParentCategoryId = c.ParentCategoryId,
            ProductCount = c.Products.Count
        });

        return Ok(categoryDtos);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoryTree()
    {
        var rootCategories = await _unitOfWork.Categories.GetRootCategoriesAsync();
        var allCategories = await _unitOfWork.Categories.GetCategoriesWithProductCountAsync();

        var categoryDtos = rootCategories.Select(c => MapToCategoryDto(c, allCategories.ToList())).ToList();

        return Ok(categoryDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdWithProductsAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var allCategories = await _unitOfWork.Categories.GetCategoriesWithProductCountAsync();
        var dto = MapToCategoryDto(category, allCategories.ToList());

        return Ok(dto);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryBySlug(string slug)
    {
        var category = await _unitOfWork.Categories.GetBySlugAsync(slug);
        if (category == null)
        {
            return NotFound();
        }

        var fullCategory = await _unitOfWork.Categories.GetByIdWithProductsAsync(category.Id);
        var allCategories = await _unitOfWork.Categories.GetCategoriesWithProductCountAsync();
        var dto = MapToCategoryDto(fullCategory!, allCategories.ToList());

        return Ok(dto);
    }

    [HttpGet("{id:guid}/subcategories")]
    public async Task<ActionResult<IEnumerable<CategoryListDto>>> GetSubCategories(Guid id)
    {
        var subCategories = await _unitOfWork.Categories.GetSubCategoriesAsync(id);

        var categoryDtos = subCategories.Select(c => new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            ImageUrl = c.ImageUrl,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ParentCategoryId = c.ParentCategoryId,
            ProductCount = c.Products.Count
        });

        return Ok(categoryDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var existingCategory = await _unitOfWork.Categories.GetBySlugAsync(request.Slug);
        if (existingCategory != null)
        {
            return BadRequest(new { Message = "A category with this slug already exists" });
        }

        var category = new Category(
            request.Name,
            request.Description,
            request.Slug,
            request.ParentCategoryId
        );

        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            category.SetImageUrl(request.ImageUrl);
        }

        category.SetDisplayOrder(request.DisplayOrder);

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryName} created with ID {CategoryId}", category.Name, category.Id);

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, MapToCategoryDto(category, new List<Category>()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var existingWithSlug = await _unitOfWork.Categories.GetBySlugAsync(request.Slug);
        if (existingWithSlug != null && existingWithSlug.Id != id)
        {
            return BadRequest(new { Message = "A category with this slug already exists" });
        }

        category.UpdateDetails(request.Name, request.Description, request.Slug);
        category.SetImageUrl(request.ImageUrl);
        category.SetDisplayOrder(request.DisplayOrder);
        category.SetParentCategory(request.ParentCategoryId);

        if (request.IsActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
        }

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} updated", id);

        return Ok(MapToCategoryDto(category, new List<Category>()));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdWithProductsAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            return BadRequest(new { Message = "Cannot delete category with products. Move or delete products first." });
        }

        var subCategories = await _unitOfWork.Categories.GetSubCategoriesAsync(id);
        if (subCategories.Any())
        {
            return BadRequest(new { Message = "Cannot delete category with subcategories. Delete subcategories first." });
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} deleted", id);

        return NoContent();
    }

    private CategoryDto MapToCategoryDto(Category category, List<Category> allCategories)
    {
        var subCategories = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();
        var parentCategory = category.ParentCategoryId.HasValue
            ? allCategories.FirstOrDefault(c => c.Id == category.ParentCategoryId)
            : null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentCategory?.Name,
            ProductCount = category.Products.Count,
            SubCategories = subCategories.Select(c => MapToCategoryDto(c, allCategories)).ToList()
        };
    }
}
