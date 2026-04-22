namespace PharmacyApp.Infrastructure.Seeds;

public static class ContentPageSeedsConfiguration
{
    public static IEnumerable<object> GetSeedData()
    {
        var seedTime = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        return
        [
            new
            {
                Id = 1,
                Slug = "license-agreement",
                Title = "License Agreement",
                Content = "This is a starter license agreement page.\nReplace this text from the admin panel before production.",
                IsPublished = true,
                CreatedAt = seedTime,
                UpdatedAt = seedTime,
                UpdatedBy = "system"
            },
            new
            {
                Id = 2,
                Slug = "contacts",
                Title = "Contacts",
                Content = "Email: support@pharmacyapp.local\nPhone: +1 (000) 000-0000\nAddress: Replace with your real support address.",
                IsPublished = true,
                CreatedAt = seedTime,
                UpdatedAt = seedTime,
                UpdatedBy = "system"
            },
            new
            {
                Id = 3,
                Slug = "about",
                Title = "About Company",
                Content = "Tell customers who you are, where you operate, and why they can trust your pharmacy.",
                IsPublished = true,
                CreatedAt = seedTime,
                UpdatedAt = seedTime,
                UpdatedBy = "system"
            }
        ];
    }
}
