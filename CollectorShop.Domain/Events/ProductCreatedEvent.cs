using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class ProductCreatedEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }

    public ProductCreatedEvent(Guid productId, string productName, string sku)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
    }
}
