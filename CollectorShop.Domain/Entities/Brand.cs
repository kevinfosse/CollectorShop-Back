using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Slug { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Brand()
    {
        Name = null!;
        Description = null!;
        Slug = null!;
    }

    public Brand(string name, string description, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));

        Name = name;
        Description = description ?? string.Empty;
        Slug = slug.ToLowerInvariant();
        IsActive = true;
    }

    public void UpdateDetails(string name, string description, string slug)
    {
        Name = name;
        Description = description;
        Slug = slug.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLogoUrl(string? logoUrl)
    {
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWebsiteUrl(string? websiteUrl)
    {
        WebsiteUrl = websiteUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
