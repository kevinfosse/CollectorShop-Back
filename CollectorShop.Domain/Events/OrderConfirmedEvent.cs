using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class OrderConfirmedEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }

    public OrderConfirmedEvent(Guid orderId, string orderNumber, Guid customerId)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
    }
}
