using CollectorShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectorShop.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Carrier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.TrackingUrl)
            .HasMaxLength(500);

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.TrackingNumber);
    }
}
