namespace CollectorShop.Infrastructure.Data.Seeding.Models;

public class BrandSeedDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
