using System.Security.Claims;
using CollectorShop.API.DTOs.Common;
using CollectorShop.API.DTOs.Orders;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Enums;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrdersController> _logger;
    private readonly IConfiguration _configuration;

    public OrdersController(IUnitOfWork unitOfWork, ILogger<OrdersController> logger, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderListDto>>> GetOrders([FromQuery] OrderFilterRequest filter)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        // Regular users can only see their own orders
        if (!User.IsInRole("Admin"))
        {
            filter.CustomerId = customerId;
        }

        var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.CustomerId,
            filter.Status,
            filter.FromDate,
            filter.ToDate,
            filter.SortBy,
            filter.SortDescending
        );

        var orderDtos = orders.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status,
            CustomerName = o.Customer?.FullName ?? string.Empty,
            TotalAmount = o.TotalAmount.Amount,
            Currency = o.TotalAmount.Currency,
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt
        });

        return Ok(new PagedResponse<OrderListDto>
        {
            Items = orderDtos,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var customerId = await GetCurrentCustomerIdAsync();
        if (!User.IsInRole("Admin") && order.CustomerId != customerId)
        {
            return Forbid();
        }

        return Ok(MapToOrderDto(order));
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
        if (order == null)
        {
            return NotFound();
        }

        var customerId = await GetCurrentCustomerIdAsync();
        if (!User.IsInRole("Admin") && order.CustomerId != customerId)
        {
            return Forbid();
        }

        var fullOrder = await _unitOfWork.Orders.GetByIdWithDetailsAsync(order.Id);
        return Ok(MapToOrderDto(fullOrder!));
    }

    [HttpPost("preview")]
    public async Task<ActionResult<OrderPreviewResponse>> PreviewOrder([FromBody] OrderPreviewRequest request)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null || !cart.Items.Any())
        {
            return BadRequest(new { Message = "Cart is empty" });
        }

        var subtotal = cart.Items.Sum(i => i.UnitPrice.Amount * i.Quantity);
        var currency = cart.Items.First().UnitPrice.Currency;
        var shippingCost = _configuration.GetValue<decimal>("OrderSettings:DefaultShippingCost", 25m);
        var taxRate = _configuration.GetValue<decimal>("OrderSettings:TaxRate", 0.20m);

        decimal discountAmount = 0;
        string? couponMessage = null;
        bool isCouponValid = false;

        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(request.CouponCode);
            if (coupon != null && coupon.IsValid())
            {
                var cartSubTotalMoney = new Money(subtotal, currency);
                discountAmount = coupon.CalculateDiscount(cartSubTotalMoney).Amount;
                isCouponValid = true;
                couponMessage = $"Coupon applied: -{discountAmount:F2} {currency}";
            }
            else
            {
                couponMessage = "Invalid or expired coupon code";
            }
        }

        var taxableAmount = subtotal - discountAmount;
        var taxAmount = taxableAmount * taxRate;
        var total = taxableAmount + shippingCost + taxAmount;

        return Ok(new OrderPreviewResponse
        {
            SubTotal = subtotal,
            ShippingCost = shippingCost,
            TaxRate = taxRate,
            TaxAmount = Math.Round(taxAmount, 2),
            DiscountAmount = discountAmount,
            Total = Math.Round(total, 2),
            Currency = currency,
            CouponMessage = couponMessage,
            IsCouponValid = isCouponValid
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound(new { Message = "Customer profile not found" });
        }

        var cart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value);
        if (cart == null || !cart.Items.Any())
        {
            return BadRequest(new { Message = "Cart is empty" });
        }

        // Validate stock availability
        foreach (var item in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null || product.AvailableQuantity < item.Quantity)
            {
                return BadRequest(new { Message = $"Insufficient stock for product: {item.Product?.Name ?? item.ProductId.ToString()}" });
            }
        }

        var shippingAddress = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.ZipCode
        );

        var billingAddress = new Address(
            request.BillingAddress.Street,
            request.BillingAddress.City,
            request.BillingAddress.State,
            request.BillingAddress.Country,
            request.BillingAddress.ZipCode
        );

        var order = new Order(
            customerId.Value,
            shippingAddress,
            billingAddress,
            request.CouponCode,
            request.Notes
        );

        // Add items from cart
        foreach (var cartItem in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
            if (product != null)
            {
                order.AddItem(product, cartItem.Quantity, cartItem.UnitPrice);
                product.ReserveStock(cartItem.Quantity);
            }
        }

        // Apply coupon if provided
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(request.CouponCode);
            if (coupon != null && coupon.IsValid())
            {
                var discount = coupon.CalculateDiscount(order.SubTotal);
                order.ApplyDiscount(discount);
                coupon.IncrementUsage();
            }
        }

        // Set shipping and tax from configuration
        var shippingCost = _configuration.GetValue<decimal>("OrderSettings:DefaultShippingCost", 25m);
        var taxRate = _configuration.GetValue<decimal>("OrderSettings:TaxRate", 0.20m);
        var currency = order.SubTotal.Currency;
        order.SetShippingCost(new Money(shippingCost, currency));
        var taxableAmount = order.SubTotal.Subtract(order.DiscountAmount);
        order.SetTaxAmount(new Money(Math.Round(taxableAmount.Amount * taxRate, 2), currency));

        // Create payment
        var payment = new Payment(order.Id, order.TotalAmount, request.PaymentMethod);

        await _unitOfWork.Orders.AddAsync(order);

        // Clear cart
        cart.Clear();

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderNumber} created for customer {CustomerId}", order.OrderNumber, customerId);

        var createdOrder = await _unitOfWork.Orders.GetByIdWithDetailsAsync(order.Id);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, MapToOrderDto(createdOrder!));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        switch (request.Status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Processing:
                order.StartProcessing();
                break;
            case OrderStatus.Cancelled:
                order.Cancel(request.Notes);
                // Release reserved stock
                foreach (var item in order.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.ReleaseReservedStock(item.Quantity);
                    }
                }
                break;
            case OrderStatus.Delivered:
                order.MarkAsDelivered();
                // Confirm stock removal
                foreach (var item in order.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.ReleaseReservedStock(item.Quantity);
                        product.RemoveStock(item.Quantity);
                    }
                }
                break;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderNumber} status updated to {Status}", order.OrderNumber, request.Status);

        var updatedOrder = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        return Ok(MapToOrderDto(updatedOrder!));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/ship")]
    public async Task<ActionResult<OrderDto>> ShipOrder(Guid id, [FromBody] ShipOrderRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Ship(request.TrackingNumber, request.Carrier);

        if (request.EstimatedDeliveryDate.HasValue && order.Shipment != null)
        {
            order.Shipment.SetEstimatedDeliveryDate(request.EstimatedDeliveryDate.Value);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderNumber} shipped with tracking {TrackingNumber}",
            order.OrderNumber, request.TrackingNumber);

        var updatedOrder = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        return Ok(MapToOrderDto(updatedOrder!));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, [FromBody] string? reason = null)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin") && order.CustomerId != customerId)
        {
            return Forbid();
        }

        order.Cancel(reason);

        // Release reserved stock
        foreach (var item in order.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.ReleaseReservedStock(item.Quantity);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderNumber} cancelled", order.OrderNumber);

        var updatedOrder = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
        return Ok(MapToOrderDto(updatedOrder!));
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

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.FullName ?? string.Empty,
            CustomerEmail = order.Customer?.Email?.Value ?? string.Empty,
            ShippingAddress = new AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                Country = order.ShippingAddress.Country,
                ZipCode = order.ShippingAddress.ZipCode
            },
            BillingAddress = new AddressDto
            {
                Street = order.BillingAddress.Street,
                City = order.BillingAddress.City,
                State = order.BillingAddress.State,
                Country = order.BillingAddress.Country,
                ZipCode = order.BillingAddress.ZipCode
            },
            SubTotal = order.SubTotal.Amount,
            ShippingCost = order.ShippingCost.Amount,
            TaxAmount = order.TaxAmount.Amount,
            DiscountAmount = order.DiscountAmount.Amount,
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            CouponCode = order.CouponCode,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSku = i.ProductSku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                TotalPrice = i.TotalPrice.Amount
            }).ToList(),
            Payment = order.Payment != null ? new PaymentDto
            {
                Id = order.Payment.Id,
                Amount = order.Payment.Amount.Amount,
                Currency = order.Payment.Amount.Currency,
                Method = order.Payment.Method,
                Status = order.Payment.Status,
                TransactionId = order.Payment.TransactionId,
                PaidAt = order.Payment.PaidAt
            } : null,
            Shipment = order.Shipment != null ? new ShipmentDto
            {
                Id = order.Shipment.Id,
                TrackingNumber = order.Shipment.TrackingNumber,
                Carrier = order.Shipment.Carrier,
                TrackingUrl = order.Shipment.TrackingUrl,
                ShippedAt = order.Shipment.ShippedAt,
                EstimatedDeliveryDate = order.Shipment.EstimatedDeliveryDate,
                DeliveredAt = order.Shipment.DeliveredAt
            } : null,
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CancelledAt = order.CancelledAt
        };
    }
}
