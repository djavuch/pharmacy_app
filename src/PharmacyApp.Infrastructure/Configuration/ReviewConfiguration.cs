using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Domain.Enums;

namespace PharmacyApp.Infrastructure.Configuration;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReviewStatus.Pending);

        builder.HasIndex(r => new { r.ProductId, r.Status });

        builder.HasOne(u => u.User)
            .WithMany(r => r.Reviews)
            .HasForeignKey(r => r.UserId);

        builder.HasOne(p => p.Product)
            .WithMany(r => r.Reviews)
            .HasForeignKey(r => r.ProductId);
    }
}
