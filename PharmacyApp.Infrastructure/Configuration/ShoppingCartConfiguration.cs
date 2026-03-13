using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Configuration;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCartModel>
{
    public void Configure(EntityTypeBuilder<ShoppingCartModel> builder)
    {
        builder.ToTable("ShoppingCart", t =>
        {
            t.HasCheckConstraint(
                "CK_ShoppingCart_UserOrSession",
                "\"UserId\" IS NOT NULL OR \"SessionId\" IS NOT NULL");
        });

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.UserId)
            .HasMaxLength(450)
            .IsRequired(false);  // Nullable

        builder.Property(sc => sc.SessionId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.HasOne(sc => sc.User)
            .WithMany()
            .HasForeignKey(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasIndex(sc => sc.SessionId);
        builder.HasIndex(sc => sc.UserId);
    }
}