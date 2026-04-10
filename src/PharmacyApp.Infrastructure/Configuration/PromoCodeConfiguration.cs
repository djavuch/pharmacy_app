using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.HasKey(p => p.PromoCodeId);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.DiscountType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Value)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.MaximumDiscountAmount)
            .HasPrecision(18, 2);

        builder.HasMany(p => p.PromoCodeProducts)
            .WithOne(pp => pp.PromoCode)
            .HasForeignKey(pp => pp.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PromoCodeCategories)
            .WithOne(pc => pc.PromoCode)
            .HasForeignKey(pc => pc.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.UsageHistory)
            .WithOne(u => u.PromoCode)
            .HasForeignKey(u => u.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}