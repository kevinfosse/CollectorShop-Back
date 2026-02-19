using CollectorShop.Domain.Common;
using CollectorShop.Domain.ValueObjects;

namespace CollectorShop.Domain.Entities;

public class CustomerAddress : BaseEntity
{
    public string Label { get; private set; } // e.g., "Home", "Work", "Shipping"
    public Address Address { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsBillingAddress { get; private set; }
    public bool IsShippingAddress { get; private set; }

    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    private CustomerAddress()
    {
        Label = null!;
        Address = null!;
    }

    public CustomerAddress(
        Guid customerId,
        string label,
        Address address,
        bool isDefault = false,
        bool isBillingAddress = true,
        bool isShippingAddress = true)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty", nameof(label));

        CustomerId = customerId;
        Label = label;
        Address = address;
        IsDefault = isDefault;
        IsBillingAddress = isBillingAddress;
        IsShippingAddress = isShippingAddress;
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLabel(string label)
    {
        Label = label;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFlags(bool isBillingAddress, bool isShippingAddress)
    {
        IsBillingAddress = isBillingAddress;
        IsShippingAddress = isShippingAddress;
        UpdatedAt = DateTime.UtcNow;
    }
}
