using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Events;

public class LowStockEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }
    public int CurrentStock { get; }
    public int Threshold { get; }

    public LowStockEvent(Guid productId, string productName, string sku, int currentStock, int threshold)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        CurrentStock = currentStock;
        Threshold = threshold;
    }
}
