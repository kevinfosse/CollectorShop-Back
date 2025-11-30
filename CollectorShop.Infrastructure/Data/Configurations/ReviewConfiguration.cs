using CollectorShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectorShop.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Title)
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.IsApproved);
        builder.HasIndex(r => new { r.CustomerId, r.ProductId })
            .IsUnique();
    }
}
