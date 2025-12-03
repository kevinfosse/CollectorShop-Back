using CollectorShop.Domain.Common;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string ProductName { get; private set; } // Snapshot at time of order
    public string ProductSku { get; private set; } // Snapshot at time of order
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    private OrderItem()
    {
        ProductName = null!;
        ProductSku = null!;
        UnitPrice = null!;
    }

    public OrderItem(Guid orderId, Guid productId, string productName, string productSku, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
