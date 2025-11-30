using CollectorShop.API.DTOs.Coupons;
using CollectorShop.Domain.Entities;
using FluentValidation;

namespace CollectorShop.API.Validators.Coupons;

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required")
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters")
            .Matches("^[A-Za-z0-9-_]+$").WithMessage("Coupon code can only contain alphanumeric characters, hyphens, and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid coupon type");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than 0");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100).WithMessage("Percentage discount cannot exceed 100%")
            .When(x => x.Type == CouponType.Percentage);

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount must be non-negative")
            .When(x => x.MinimumOrderAmount.HasValue);

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThan(0).WithMessage("Maximum discount amount must be greater than 0")
            .When(x => x.MaximumDiscountAmount.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("Usage limit must be greater than 0")
            .When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.UsageLimitPerCustomer)
            .GreaterThan(0).WithMessage("Usage limit per customer must be greater than 0")
            .When(x => x.UsageLimitPerCustomer.HasValue);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.StartsAt).WithMessage("Expiry date must be after start date")
            .When(x => x.StartsAt.HasValue && x.ExpiresAt.HasValue);
    }
}
