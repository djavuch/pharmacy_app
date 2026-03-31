using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class BonusAccountConfiguration : IEntityTypeConfiguration<BonusAccountModel>
{
    public void Configure(EntityTypeBuilder<BonusAccountModel> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.UserId).IsUnique();

        builder.Property(b => b.Balance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.HasOne(b => b.User)
            .WithOne(u => u.BonusAccount)
            .HasForeignKey<BonusAccountModel>(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Transactions)
            .WithOne(t => t.BonusAccount)
            .HasForeignKey(t => t.BonusAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}