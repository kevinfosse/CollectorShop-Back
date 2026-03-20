using System.Security.Claims;
using CollectorShop.API.DTOs.Cart;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CartController> _logger;

    public CartController(IUnitOfWork unitOfWork, ILogger<CartController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null)
        {
            return Ok(new CartDto
            {
                CustomerId = customerId.Value,
                Items = new List<CartItemDto>(),
                TotalAmount = 0,
                TotalItems = 0
            });
        }

        return Ok(MapToCartDto(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            return NotFound(new { Message = "Product not found" });
        }

        if (!product.IsActive)
        {
            return BadRequest(new { Message = "Product is not available" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null)
        {
            cart = new Cart(customerId.Value);
            await _unitOfWork.Carts.AddAsync(cart);
        }

        var existingQuantity = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId)?.Quantity ?? 0;
        var totalQuantity = existingQuantity + request.Quantity;
        if (product.AvailableQuantity < totalQuantity)
        {
            return BadRequest(new { Message = $"Insufficient stock. Available: {product.AvailableQuantity}, in cart: {existingQuantity}, requested: {request.Quantity}" });
        }

        cart.AddItem(product, request.Quantity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Added product {ProductId} to cart for customer {CustomerId}", request.ProductId, customerId);

        return Ok(MapToCartDto(cart));
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid productId, [FromBody] UpdateCartItemRequest request)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null)
        {
            return NotFound(new { Message = "Cart not found" });
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product != null && product.AvailableQuantity < request.Quantity)
        {
            return BadRequest(new { Message = "Insufficient stock available" });
        }

        cart.UpdateItemQuantity(productId, request.Quantity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated cart item {ProductId} quantity to {Quantity} for customer {CustomerId}",
            productId, request.Quantity, customerId);

        return Ok(MapToCartDto(cart));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveFromCart(Guid productId)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null)
        {
            return NotFound(new { Message = "Cart not found" });
        }

        cart.RemoveItem(productId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Removed product {ProductId} from cart for customer {CustomerId}", productId, customerId);

        return Ok(MapToCartDto(cart));
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null)
        {
            return NotFound(new { Message = "Cart not found" });
        }

        cart.Clear();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cart cleared for customer {CustomerId}", customerId);

        return NoContent();
    }

    private async Task<Guid?> GetCurrentCustomerIdAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        return customer?.Id;
    }

    private static CartDto MapToCartDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                ProductSku = i.Product?.Sku ?? string.Empty,
                ProductImageUrl = i.Product?.Images.FirstOrDefault(img => img.IsPrimary)?.Url
                    ?? i.Product?.Images.FirstOrDefault()?.Url,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                TotalPrice = i.TotalPrice.Amount,
                Currency = i.UnitPrice.Currency,
                AvailableStock = i.Product?.AvailableQuantity ?? 0
            }).ToList(),
            TotalAmount = cart.TotalAmount.Amount,
            Currency = cart.TotalAmount.Currency,
            TotalItems = cart.TotalItems,
            ExpiresAt = cart.ExpiresAt
        };
    }
}
