using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class PromoCodeUsageConfiguration : IEntityTypeConfiguration<PromoCodeUsageModel>
{
    public void Configure(EntityTypeBuilder<PromoCodeUsageModel> builder)
    { 
        builder.HasKey(u => u.UsageId);

        builder.Property(u => u.DiscountApplied)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(u => new { u.PromoCodeId, u.UserId });
        builder.HasIndex(u => u.OrderId);
    }
}