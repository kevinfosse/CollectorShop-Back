using CollectorShop.Domain.Common;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string? UserId { get; private set; } // Link to ASP.NET Identity

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    public Cart? Cart { get; private set; }

    private readonly List<WishlistItem> _wishlistItems = new();
    public IReadOnlyCollection<WishlistItem> WishlistItems => _wishlistItems.AsReadOnly();

    private Customer()
    {
        FirstName = null!;
        LastName = null!;
        Email = null!;
    }

    public Customer(string firstName, string lastName, Email email, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserId = userId;
        IsActive = true;
        IsEmailVerified = false;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, PhoneNumber? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(Email email)
    {
        Email = email;
        IsEmailVerified = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void AddToWishlist(WishlistItem item)
    {
        if (!_wishlistItems.Any(w => w.ProductId == item.ProductId))
        {
            _wishlistItems.Add(item);
        }
    }

    public void RemoveFromWishlist(Guid productId)
    {
        var item = _wishlistItems.FirstOrDefault(w => w.ProductId == productId);
        if (item != null)
        {
            _wishlistItems.Remove(item);
        }
    }
}
