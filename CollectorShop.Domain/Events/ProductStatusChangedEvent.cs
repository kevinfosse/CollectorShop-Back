using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class ProductStatusChangedEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public bool IsActive { get; }

    public ProductStatusChangedEvent(Guid productId, string productName, bool isActive)
    {
        ProductId = productId;
        ProductName = productName;
        IsActive = isActive;
    }
}
