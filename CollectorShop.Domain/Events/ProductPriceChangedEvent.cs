using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class ProductPriceChangedEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
    public string Currency { get; }

    public ProductPriceChangedEvent(Guid productId, string productName, decimal oldPrice, decimal newPrice, string currency)
    {
        ProductId = productId;
        ProductName = productName;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Currency = currency;
    }
}
