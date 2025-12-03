using CollectorShop.Domain.Common;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public CouponType Type { get; private set; }
    public decimal Value { get; private set; } // Percentage or Fixed amount
    public Money? MinimumOrderAmount { get; private set; }
    public Money? MaximumDiscountAmount { get; private set; }

    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }
    public int? UsageLimitPerCustomer { get; private set; }

    public DateTime? StartsAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    private Coupon()
    {
        Code = null!;
        Description = null!;
    }

    public Coupon(
        string code,
        string description,
        CouponType type,
        decimal value,
        Money? minimumOrderAmount = null,
        Money? maximumDiscountAmount = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Coupon code cannot be empty", nameof(code));
        if (value <= 0)
            throw new ArgumentException("Value must be positive", nameof(value));
        if (type == CouponType.Percentage && value > 100)
            throw new ArgumentException("Percentage cannot exceed 100", nameof(value));

        Code = code.ToUpperInvariant();
        Description = description ?? string.Empty;
        Type = type;
        Value = value;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;
        IsActive = true;
        UsageCount = 0;
    }

    public bool IsValid()
    {
        if (!IsActive) return false;
        if (UsageLimit.HasValue && UsageCount >= UsageLimit.Value) return false;
        if (StartsAt.HasValue && DateTime.UtcNow < StartsAt.Value) return false;
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value) return false;
        return true;
    }

    public Money CalculateDiscount(Money orderAmount)
    {
        if (!IsValid())
            return Money.Zero(orderAmount.Currency);

        if (MinimumOrderAmount != null && orderAmount.Amount < MinimumOrderAmount.Amount)
            return Money.Zero(orderAmount.Currency);

        Money discount;
        if (Type == CouponType.Percentage)
        {
            discount = orderAmount.Multiply(Value / 100);
        }
        else
        {
            discount = new Money(Value, orderAmount.Currency);
        }

        if (MaximumDiscountAmount != null && discount.Amount > MaximumDiscountAmount.Amount)
        {
            discount = MaximumDiscountAmount;
        }

        return discount;
    }

    public void IncrementUsage()
    {
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetUsageLimit(int? limit)
    {
        UsageLimit = limit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDateRange(DateTime? startsAt, DateTime? expiresAt)
    {
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1
}
