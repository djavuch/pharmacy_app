using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.Discount;

namespace PharmacyApp.Infrastructure.Configuration.Discount;

public class ProductDiscountConfiguration : IEntityTypeConfiguration<ProductDiscountModel>
{
    public void Configure(EntityTypeBuilder<ProductDiscountModel> builder)
    {
        builder.HasKey(pd => new { pd.ProductId, pd.DiscountId });

        builder.HasOne(pd => pd.Product)
            .WithMany(p => p.ProductDiscounts)
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pd => pd.Discount)
            .WithMany(d => d.ProductDiscounts)
            .HasForeignKey(pd => pd.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}