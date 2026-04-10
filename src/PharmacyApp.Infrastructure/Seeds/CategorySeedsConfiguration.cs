using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Seeds;

public static class CategorySeedsConfiguration
{
    public static IEnumerable<object> GetSeedData()
    {
        return new List<object>
        {
            new
            {
                CategoryId = 1, CategoryName = "Vitamins & Supplements",
                CategoryDescription = "Essential vitamins and dietary supplements for your health"
            },
            new
            {
                CategoryId = 2, CategoryName = "Pain Relief",
                CategoryDescription = "Over-the-counter pain medications and relief products"
            },
            new
            {
                CategoryId = 3, CategoryName = "Cold & Flu",
                CategoryDescription = "Medications and remedies for cold and flu symptoms"
            },
            new
            {
                CategoryId = 4, CategoryName = "First Aid",
                CategoryDescription = "First aid supplies and emergency medical equipment"
            },
            new
            {
                CategoryId = 5, CategoryName = "Skincare",
                CategoryDescription = "Dermatological products and skin treatment solutions"
            },
            new
            {
                CategoryId = 6, CategoryName = "Digestive Health",
                CategoryDescription = "Products for digestive system support and health"
            },
            new
            {
                CategoryId = 7, CategoryName = "Baby Care",
                CategoryDescription = "Healthcare products for infants and toddlers"
            },
            new
            {
                CategoryId = 8, CategoryName = "Medical Devices",
                CategoryDescription = "Blood pressure monitors, thermometers, and other medical devices"
            },
            new
            {
                CategoryId = 9, CategoryName = "Eye Care",
                CategoryDescription = "Eye drops, contact lens solutions, and vision care products"
            },
            new
            {
                CategoryId = 10, CategoryName = "Oral Care",
                CategoryDescription = "Dental hygiene and oral health products"
            },
            new
            {
                CategoryId = 11, CategoryName = "Personal Care",
                CategoryDescription = "Personal hygiene and grooming products"
            },
            new
            {
                CategoryId = 12, CategoryName = "Women's Health",
                CategoryDescription = "Health products specifically for women"
            },
            new
            {
                CategoryId = 13, CategoryName = "Men's Health",
                CategoryDescription = "Health products specifically for men"
            },
            new
            {
                CategoryId = 14, CategoryName = "Diabetes Care",
                CategoryDescription = "Products for diabetes management and monitoring"
            },
            new
            {
                CategoryId = 15, CategoryName = "Heart Health",
                CategoryDescription = "Cardiovascular health support products"
            },
        };
    }
}