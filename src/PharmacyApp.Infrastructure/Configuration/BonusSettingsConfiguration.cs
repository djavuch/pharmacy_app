using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities.Bonus;

namespace PharmacyApp.Infrastructure.Data.Configurations;

public class BonusSettingsConfiguration : IEntityTypeConfiguration<BonusSettingsModel>
{
    public void Configure(EntityTypeBuilder<BonusSettingsModel> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.EarningRate).HasPrecision(5, 2);
        builder.Property(s => s.MinOrderAmountToEarn).HasPrecision(18, 2);
        builder.Property(s => s.MaxRedeemPercent).HasPrecision(5, 2);

        // first record with default values
        builder.HasData(new BonusSettingsModel
        {
            Id = 1,
            EarningRate = 1m,
            MinOrderAmountToEarn = 0m,
            MaxRedeemPercent = 100m,
            IsEarningEnabled = true,
            IsRedemptionEnabled = true,
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}