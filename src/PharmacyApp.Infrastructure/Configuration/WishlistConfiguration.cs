using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasKey(w => new { w.UserId, w.ProductId });
        builder.HasOne(w => w.User)
                .WithMany(u => u.Wishlist)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(w => w.Product)
               .WithMany(p => p.Wishlist)
               .HasForeignKey(w => w.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
