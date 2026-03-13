using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class OrderAddressConfiguration : IEntityTypeConfiguration<OrderAddressModel>
{
    public void Configure(EntityTypeBuilder<OrderAddressModel> builder)
    {
        builder.HasKey(oa => oa.AddressId);

        builder.Property(oa => oa.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oa => oa.ApartmentNumber)
            .HasMaxLength(50);

        builder.Property(oa => oa.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.ZipCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(oa => oa.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oa => oa.AdditionalInfo)
            .HasMaxLength(500);
    }
}
