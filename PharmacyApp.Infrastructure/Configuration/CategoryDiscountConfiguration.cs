using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.Discount;

namespace PharmacyApp.Infrastructure.Configuration.Discount;

public class CategoryDiscountConfiguration : IEntityTypeConfiguration<CategoryDiscountModel>
{
    public void Configure(EntityTypeBuilder<CategoryDiscountModel> builder)
    {
        builder.HasKey(cd => new { cd.CategoryId, cd.DiscountId });

        builder.HasOne(cd => cd.Category)
            .WithMany(c => c.CategoryDiscounts)
            .HasForeignKey(cd => cd.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cd => cd.Discount)
            .WithMany(d => d.CategoryDiscounts)
            .HasForeignKey(cd => cd.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}