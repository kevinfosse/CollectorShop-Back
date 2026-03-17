using CollectorShop.API.DTOs.Coupons;
using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(IUnitOfWork unitOfWork, ILogger<CouponsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CouponDto>>> GetCoupons()
    {
        var coupons = await _unitOfWork.Coupons.GetAllAsync();

        var couponDtos = coupons.Select(c => new CouponDto
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            Type = c.Type,
            Value = c.Value,
            MinimumOrderAmount = c.MinimumOrderAmount?.Amount,
            MaximumDiscountAmount = c.MaximumDiscountAmount?.Amount,
            UsageLimit = c.UsageLimit,
            UsageCount = c.UsageCount,
            UsageLimitPerCustomer = c.UsageLimitPerCustomer,
            StartsAt = c.StartsAt,
            ExpiresAt = c.ExpiresAt,
            IsActive = c.IsActive,
            IsValid = c.IsValid()
        });

        return Ok(couponDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CouponDto>> GetCoupon(Guid id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
        if (coupon == null)
        {
            return NotFound();
        }

        return Ok(new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount?.Amount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount?.Amount,
            UsageLimit = coupon.UsageLimit,
            UsageCount = coupon.UsageCount,
            UsageLimitPerCustomer = coupon.UsageLimitPerCustomer,
            StartsAt = coupon.StartsAt,
            ExpiresAt = coupon.ExpiresAt,
            IsActive = coupon.IsActive,
            IsValid = coupon.IsValid()
        });
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<ActionResult<ValidateCouponResponse>> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(request.Code);

        if (coupon == null)
        {
            return Ok(new ValidateCouponResponse
            {
                IsValid = false,
                Message = "Coupon not found"
            });
        }

        if (!coupon.IsValid())
        {
            return Ok(new ValidateCouponResponse
            {
                IsValid = false,
                Message = "Coupon is not valid or has expired"
            });
        }

        var orderAmount = new Money(request.OrderAmount, "EUR");

        if (coupon.MinimumOrderAmount != null && orderAmount.Amount < coupon.MinimumOrderAmount.Amount)
        {
            return Ok(new ValidateCouponResponse
            {
                IsValid = false,
                Message = $"Minimum order amount is {coupon.MinimumOrderAmount}"
            });
        }

        var discount = coupon.CalculateDiscount(orderAmount);

        return Ok(new ValidateCouponResponse
        {
            IsValid = true,
            Message = "Coupon is valid",
            DiscountAmount = discount.Amount
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CouponDto>> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var existingCoupon = await _unitOfWork.Coupons.GetByCodeAsync(request.Code);
        if (existingCoupon != null)
        {
            return BadRequest(new { Message = "A coupon with this code already exists" });
        }

        var coupon = new Coupon(
            request.Code,
            request.Description,
            request.Type,
            request.Value,
            request.MinimumOrderAmount.HasValue ? new Money(request.MinimumOrderAmount.Value, "EUR") : null,
            request.MaximumDiscountAmount.HasValue ? new Money(request.MaximumDiscountAmount.Value, "EUR") : null
        );

        coupon.SetUsageLimit(request.UsageLimit);
        coupon.SetDateRange(request.StartsAt, request.ExpiresAt);

        await _unitOfWork.Coupons.AddAsync(coupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Coupon {CouponCode} created with ID {CouponId}", coupon.Code, coupon.Id);

        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount?.Amount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount?.Amount,
            UsageLimit = coupon.UsageLimit,
            UsageCount = coupon.UsageCount,
            UsageLimitPerCustomer = coupon.UsageLimitPerCustomer,
            StartsAt = coupon.StartsAt,
            ExpiresAt = coupon.ExpiresAt,
            IsActive = coupon.IsActive,
            IsValid = coupon.IsValid()
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CouponDto>> UpdateCoupon(Guid id, [FromBody] UpdateCouponRequest request)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
        if (coupon == null)
        {
            return NotFound();
        }

        coupon.SetUsageLimit(request.UsageLimit);
        coupon.SetDateRange(request.StartsAt, request.ExpiresAt);

        if (request.IsActive)
        {
            coupon.Activate();
        }
        else
        {
            coupon.Deactivate();
        }

        _unitOfWork.Coupons.Update(coupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Coupon {CouponId} updated", id);

        return Ok(new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            Type = coupon.Type,
            Value = coupon.Value,
            MinimumOrderAmount = coupon.MinimumOrderAmount?.Amount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount?.Amount,
            UsageLimit = coupon.UsageLimit,
            UsageCount = coupon.UsageCount,
            UsageLimitPerCustomer = coupon.UsageLimitPerCustomer,
            StartsAt = coupon.StartsAt,
            ExpiresAt = coupon.ExpiresAt,
            IsActive = coupon.IsActive,
            IsValid = coupon.IsValid()
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCoupon(Guid id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
        if (coupon == null)
        {
            return NotFound();
        }

        _unitOfWork.Coupons.Remove(coupon);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Coupon {CouponId} deleted", id);

        return NoContent();
    }
}
