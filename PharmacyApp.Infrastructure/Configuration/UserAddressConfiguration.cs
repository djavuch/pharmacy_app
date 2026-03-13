using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddressModel>
{
    public void Configure(EntityTypeBuilder<UserAddressModel> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.ApartmentNumber)
            .HasMaxLength(50);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ZipCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.AdditionalInfo)
            .HasMaxLength(500);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.UserId, a.IsDefault });
    }
}
