using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => new { ci.CartId, ci.ProductId });

        builder.HasOne(ci => ci.ShoppingCart)
            .WithMany(sc => sc.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ci => ci.PriceAtAdd)
            .HasPrecision(18, 2);
    }
}