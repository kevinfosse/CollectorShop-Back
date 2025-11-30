using CollectorShop.Domain.Common;
using CollectorShop.Domain.Enums;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }

    public string? TransactionId { get; private set; }
    public string? PaymentIntentId { get; private set; }
    public string? FailureReason { get; private set; }

    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, Money amount, PaymentMethod method)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
    }

    public void SetPaymentIntentId(string paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Authorize(string transactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be authorized");

        TransactionId = transactionId;
        Status = PaymentStatus.Authorized;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException("Only authorized payments can be captured");

        Status = PaymentStatus.Captured;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException("Only captured payments can be refunded");

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PartialRefund()
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException("Only captured payments can be partially refunded");

        Status = PaymentStatus.PartiallyRefunded;
        UpdatedAt = DateTime.UtcNow;
    }
}
