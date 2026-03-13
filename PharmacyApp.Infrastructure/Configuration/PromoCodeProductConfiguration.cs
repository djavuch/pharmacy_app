using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.PromoCode;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class PromoCodeProductConfiguration : IEntityTypeConfiguration<PromoCodeProductModel>
{
    public void Configure(EntityTypeBuilder<PromoCodeProductModel> builder)
    {
        builder.HasKey(pp => new { pp.PromoCodeId, pp.ProductId });
    }
}