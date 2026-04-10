using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Seeds;

namespace PharmacyApp.Infrastructure.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(pc => pc.CategoryId);

        builder.Property(pc => pc.CategoryName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(pc => pc.CategoryDescription)
               .HasMaxLength(800);

        builder.HasMany(pc => pc.Products)
               .WithOne(p => p.Category)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(CategorySeedsConfiguration.GetSeedData());
    }
}
