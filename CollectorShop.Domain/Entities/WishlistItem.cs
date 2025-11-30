using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public DateTime AddedAt { get; private set; }

    private WishlistItem() { }

    public WishlistItem(Guid customerId, Guid productId)
    {
        CustomerId = customerId;
        ProductId = productId;
        AddedAt = DateTime.UtcNow;
    }
}
