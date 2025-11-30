using CollectorShop.API.DTOs.Coupons;
using FluentValidation;

namespace CollectorShop.API.Validators.Coupons;

public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

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
