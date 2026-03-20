using CollectorShop.Domain.Common;
using CollectorShop.Domain.Enums;

namespace CollectorShop.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Comment { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public ReviewStatus Status { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    private Review() { }

    public Review(Guid productId, Guid customerId, int rating, string? title = null, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        ProductId = productId;
        CustomerId = customerId;
        Rating = rating;
        Title = title;
        Comment = comment;
        IsVerifiedPurchase = false;
        Status = ReviewStatus.Pending;
    }

    public void UpdateReview(int rating, string? title, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        Rating = rating;
        Title = title;
        Comment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsVerifiedPurchase() => IsVerifiedPurchase = true;
    public void Approve() => Status = ReviewStatus.Approved;
    public void Reject() => Status = ReviewStatus.Rejected;
}
