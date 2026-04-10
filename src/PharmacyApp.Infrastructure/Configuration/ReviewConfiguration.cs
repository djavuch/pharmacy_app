using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.HasOne(u => u.User)
            .WithMany(r => r.Reviews)
            .HasForeignKey(r => r.UserId);

        builder.HasOne(p => p.Product)
            .WithMany(r => r.Reviews)
            .HasForeignKey(r => r.ProductId);
    }
}
