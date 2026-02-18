using CollectorShop.Domain.Common;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public DateTime? ExpiresAt { get; private set; }

    private Cart() { }

    public Cart(Guid customerId)
    {
        CustomerId = customerId;
        ExpiresAt = DateTime.UtcNow.AddDays(30);
    }

    public Money TotalAmount
    {
        get
        {
            if (!_items.Any())
                return Money.Zero();

            return _items.Aggregate(Money.Zero(), (total, item) => total.Add(item.TotalPrice));
        }
    }

    public int TotalItems => _items.Sum(i => i.Quantity);

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = new CartItem(Id, product.Id, quantity, product.Price);
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
        ExtendExpiration();
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Item not found in cart");

        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(quantity);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    private void ExtendExpiration()
    {
        ExpiresAt = DateTime.UtcNow.AddDays(30);
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}
