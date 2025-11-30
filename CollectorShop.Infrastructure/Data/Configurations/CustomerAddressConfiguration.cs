using CollectorShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectorShop.Infrastructure.Data.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.Label)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(ca => ca.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("Street")
                .IsRequired()
                .HasMaxLength(200);
            addressBuilder.Property(a => a.City)
                .HasColumnName("City")
                .IsRequired()
                .HasMaxLength(100);
            addressBuilder.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100);
            addressBuilder.Property(a => a.Country)
                .HasColumnName("Country")
                .IsRequired()
                .HasMaxLength(100);
            addressBuilder.Property(a => a.ZipCode)
                .HasColumnName("ZipCode")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.HasOne(ca => ca.Customer)
            .WithMany()
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ca => ca.CustomerId);
    }
}
