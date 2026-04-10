using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasOne(u => u.User)
               .WithMany(o => o.Orders)
               .HasForeignKey(o => o.UserId);
        
        builder.HasMany(o => o.OrderItems)
               .WithOne(oi => oi.Order)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oa => oa.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.AppliedPromoCode)
             .HasMaxLength(50);

        builder.Property(o => o.PromoCodeDiscountAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(o => o.RowVersion).IsRowVersion();
    }
}