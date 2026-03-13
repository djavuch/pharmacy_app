using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.Discount;

namespace PharmacyApp.Infrastructure.Configuration.Discount;

public class DiscountConfiguration : IEntityTypeConfiguration<DiscountModel>
{
    public void Configure(EntityTypeBuilder<DiscountModel> builder)
    {
        builder.HasKey(d => d.DiscountId);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.DiscountType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(d => d.StartDate)
            .IsRequired();

        builder.Property(d => d.EndDate)
            .IsRequired();

        builder.Property(d => d.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(d => d.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(d => d.MaximumOrderAmount)
            .HasPrecision(18, 2);

        // Indexes 
        builder.HasIndex(d => new { d.IsActive, d.StartDate, d.EndDate })
            .HasDatabaseName("IX_Discount_Active_Dates");

        builder.HasIndex(d => d.IsActive)
            .HasDatabaseName("IX_Discount_IsActive");

        builder.HasMany(d => d.ProductDiscounts)
            .WithOne(pd => pd.Discount)
            .HasForeignKey(pd => pd.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.CategoryDiscounts)
            .WithOne(cd => cd.Discount)
            .HasForeignKey(cd => cd.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}