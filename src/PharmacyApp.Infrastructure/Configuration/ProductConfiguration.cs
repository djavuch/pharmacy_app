using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Seeds;

namespace PharmacyApp.Infrastructure.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductCode)
            .HasMaxLength(32)
            .IsRequired(false);

        builder.HasIndex(p => p.ProductCode)
            .IsUnique();

        builder
            .HasMany(r => r.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId);

        builder.Property(p => p.WishlistCount)
            .HasDefaultValue(0)
            .IsRequired();
        
        builder.HasData(ProductSeedsConfiguration.GetSeedData());
    }
}
