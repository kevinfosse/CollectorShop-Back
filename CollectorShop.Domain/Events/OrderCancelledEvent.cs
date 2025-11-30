using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class OrderCancelledEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }
    public string? Reason { get; }

    public OrderCancelledEvent(Guid orderId, string orderNumber, Guid customerId, string? reason)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Reason = reason;
    }
}
