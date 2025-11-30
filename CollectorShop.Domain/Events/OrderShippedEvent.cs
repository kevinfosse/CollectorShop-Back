using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class OrderShippedEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }
    public string TrackingNumber { get; }

    public OrderShippedEvent(Guid orderId, string orderNumber, Guid customerId, string trackingNumber)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        TrackingNumber = trackingNumber;
    }
}
