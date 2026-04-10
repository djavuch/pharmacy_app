using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class PromoCodeCategoryConfiguration : IEntityTypeConfiguration<PromoCodeCategory>
{
    public void Configure(EntityTypeBuilder<PromoCodeCategory> builder)
    {
        builder.HasKey(pc => new { pc.PromoCodeId, pc.CategoryId });
    }
}