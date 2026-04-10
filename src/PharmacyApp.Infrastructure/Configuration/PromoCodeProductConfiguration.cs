using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class PromoCodeProductConfiguration : IEntityTypeConfiguration<PromoCodeProduct>
{
    public void Configure(EntityTypeBuilder<PromoCodeProduct> builder)
    {
        builder.HasKey(pp => new { pp.PromoCodeId, pp.ProductId });
    }
}