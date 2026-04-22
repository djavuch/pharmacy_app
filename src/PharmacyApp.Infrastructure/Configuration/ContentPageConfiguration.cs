using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyApp.Domain.Entities;
using PharmacyApp.Infrastructure.Seeds;

namespace PharmacyApp.Infrastructure.Configuration;

public class ContentPageConfiguration : IEntityTypeConfiguration<ContentPage>
{
    public void Configure(EntityTypeBuilder<ContentPage> builder)
    {
        builder.HasKey(page => page.Id);

        builder.Property(page => page.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(page => page.Slug)
            .IsUnique();

        builder.Property(page => page.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(page => page.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(page => page.IsPublished)
            .IsRequired();

        builder.Property(page => page.CreatedAt)
            .IsRequired();

        builder.Property(page => page.UpdatedAt)
            .IsRequired();

        builder.Property(page => page.UpdatedBy)
            .HasMaxLength(256);

        builder.HasData(ContentPageSeedsConfiguration.GetSeedData());
    }
}
