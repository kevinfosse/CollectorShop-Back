using CollectorShop.API.DTOs.Products;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public WishlistController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<List<WishlistItemDto>>> GetWishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var wishlistItems = customer.WishlistItems.Select(w => new WishlistItemDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.Product?.Name ?? string.Empty,
            ProductSku = w.Product?.Sku ?? string.Empty,
            Price = w.Product?.Price.Amount ?? 0,
            Currency = w.Product?.Price.Currency ?? "USD",
            ImageUrl = w.Product?.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                      ?? w.Product?.Images.FirstOrDefault()?.Url,
            IsInStock = (w.Product?.AvailableQuantity ?? 0) > 0,
            AddedAt = w.AddedAt
        }).ToList();

        return Ok(wishlistItems);
    }

    [HttpPost]
    public async Task<ActionResult<WishlistItemDto>> AddToWishlist([FromBody] AddToWishlistRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        // Check if already in wishlist
        if (customer.WishlistItems.Any(w => w.ProductId == request.ProductId))
        {
            return BadRequest("Product is already in your wishlist");
        }

        var wishlistItem = new WishlistItem(customer.Id, request.ProductId);
        customer.AddToWishlist(wishlistItem);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWishlist), new WishlistItemDto
        {
            Id = wishlistItem.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSku = product.Sku,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            ImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                      ?? product.Images.FirstOrDefault()?.Url,
            IsInStock = product.AvailableQuantity > 0,
            AddedAt = wishlistItem.AddedAt
        });
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var wishlistItem = customer.WishlistItems.FirstOrDefault(w => w.ProductId == productId);
        if (wishlistItem == null)
        {
            return NotFound("Product not found in wishlist");
        }

        customer.RemoveFromWishlist(productId);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{productId:guid}/move-to-cart")]
    public async Task<IActionResult> MoveToCart(Guid productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Customer profile not found");
        }

        var wishlistItem = customer.WishlistItems.FirstOrDefault(w => w.ProductId == productId);
        if (wishlistItem == null)
        {
            return NotFound("Product not found in wishlist");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        if (product.AvailableQuantity <= 0)
        {
            return BadRequest("Product is out of stock");
        }

        // Get or create cart
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customer.Id);
        if (cart == null)
        {
            cart = new Cart(customer.Id);
            await _unitOfWork.Carts.AddAsync(cart);
        }

        cart.AddItem(product, 1);
        customer.RemoveFromWishlist(productId);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Product moved to cart successfully" });
    }
}

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string? ImageUrl { get; set; }
    public bool IsInStock { get; set; }
    public DateTime AddedAt { get; set; }
}

public class AddToWishlistRequest
{
    public Guid ProductId { get; set; }
}