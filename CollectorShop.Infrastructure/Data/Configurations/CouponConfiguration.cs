using CollectorShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectorShop.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.OwnsOne(c => c.MinimumOrderAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("MinimumOrderAmount")
                .HasPrecision(18, 2);
            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("MinimumOrderAmountCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(c => c.MaximumDiscountAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("MaximumDiscountAmount")
                .HasPrecision(18, 2);
            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("MaximumDiscountAmountCurrency")
                .HasMaxLength(3);
        });

        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.ExpiresAt);
    }
}
