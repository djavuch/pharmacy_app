using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class BonusTransactionConfiguration : IEntityTypeConfiguration<BonusTransactionModel>
{
    public void Configure(EntityTypeBuilder<BonusTransactionModel> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Points).HasPrecision(18, 2);

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Description).HasMaxLength(500);

        builder.HasIndex(t => t.OrderId);
    }
}