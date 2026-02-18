using CollectorShop.Domain.Common;
using CollectorShop.Domain.Enums;
using CollectorShop.Domain.Events;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }

    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    public Address ShippingAddress { get; private set; }
    public Address BillingAddress { get; private set; }

    public Money SubTotal { get; private set; }
    public Money ShippingCost { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }

    public string? CouponCode { get; private set; }
    public string? Notes { get; private set; }

    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Payment? Payment { get; private set; }

    public Shipment? Shipment { get; private set; }

    private Order()
    {
        OrderNumber = null!;
        ShippingAddress = null!;
        BillingAddress = null!;
        SubTotal = null!;
        ShippingCost = null!;
        TaxAmount = null!;
        DiscountAmount = null!;
        TotalAmount = null!;
    }

    public Order(
        Guid customerId,
        Address shippingAddress,
        Address billingAddress,
        string? couponCode = null,
        string? notes = null)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        CouponCode = couponCode;
        Notes = notes;
        Status = OrderStatus.Pending;
        OrderNumber = GenerateOrderNumber();

        SubTotal = Money.Zero();
        ShippingCost = Money.Zero();
        TaxAmount = Money.Zero();
        DiscountAmount = Money.Zero();
        TotalAmount = Money.Zero();
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }

    public void AddItem(Product product, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = new OrderItem(Id, product.Id, product.Name, product.Sku, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotals();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
        }
    }

    public void SetShippingCost(Money shippingCost)
    {
        ShippingCost = shippingCost;
        RecalculateTotals();
    }

    public void SetTaxAmount(Money taxAmount)
    {
        TaxAmount = taxAmount;
        RecalculateTotals();
    }

    public void ApplyDiscount(Money discountAmount)
    {
        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        var currency = _items.FirstOrDefault()?.TotalPrice.Currency ?? "USD";
        SubTotal = _items.Any()
            ? _items.Aggregate(Money.Zero(currency), (total, item) => total.Add(item.TotalPrice))
            : Money.Zero(currency);

        // Ensure related amounts use the same currency
        if (ShippingCost.Currency != currency)
            ShippingCost = Money.Zero(currency);
        if (TaxAmount.Currency != currency)
            TaxAmount = Money.Zero(currency);
        if (DiscountAmount.Currency != currency)
            DiscountAmount = Money.Zero(currency);

        TotalAmount = SubTotal
            .Add(ShippingCost)
            .Add(TaxAmount)
            .Subtract(DiscountAmount);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an order without items");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderConfirmedEvent(Id, OrderNumber, CustomerId));
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can start processing");

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship(string trackingNumber, string carrier)
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        Shipment = new Shipment(Id, trackingNumber, carrier);

        AddDomainEvent(new OrderShippedEvent(Id, OrderNumber, CustomerId, trackingNumber));
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be marked as delivered");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (Shipment != null)
        {
            Shipment.MarkAsDelivered();
        }

        AddDomainEvent(new OrderDeliveredEvent(Id, OrderNumber, CustomerId));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel shipped or delivered orders");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes}\nCancellation Reason: {reason}";
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, CustomerId, reason));
    }

    public void Refund()
    {
        if (Status != OrderStatus.Delivered && Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only delivered or shipped orders can be refunded");

        Status = OrderStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }
}
