using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Seeds;

public static class ProductSeedsConfiguration
{
    public static IEnumerable<ProductModel> GetSeedData()
    {
        var products = new List<ProductModel>();
        int productId = 1;
        // Vitamins & Supplements (CategoryId = 1)
        products.AddRange(
        [
            new ProductModel
            {
                Id = productId++,
                Name = "Vitamin C 1000mg",
                Description = "High-potency vitamin C tablets for immune support. Take 1 tablet daily with food.",
                Price = 15.99m,
                StockQuantity = 200,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FF6B6B/FFFFFF?text=Vitamin+C",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Vitamin D3 5000 IU",
                Description = "Essential vitamin D for bone health and immune function. Supports calcium absorption.",
                Price = 18.99m,
                StockQuantity = 150,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/4ECDC4/FFFFFF?text=Vitamin+D3",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Multivitamin Complex",
                Description = "Daily multivitamin with essential nutrients for overall health and wellness.",
                Price = 24.99m,
                StockQuantity = 180,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/95E1D3/FFFFFF?text=Multivitamin",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Omega-3 Fish Oil 1200mg",
                Description = "High-quality omega-3 fatty acids for heart and brain health.",
                Price = 29.99m,
                StockQuantity = 120,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/F38181/FFFFFF?text=Omega-3",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Calcium + Magnesium",
                Description = "Bone support formula with vitamin D3 for optimal absorption.",
                Price = 19.99m,
                StockQuantity = 160,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/AA96DA/FFFFFF?text=Calcium",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Zinc 50mg",
                Description = "Immune system support and wound healing. Antioxidant properties.",
                Price = 12.99m,
                StockQuantity = 220,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FCBAD3/FFFFFF?text=Zinc",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Iron Supplement 65mg",
                Description = "Gentle iron formula for energy and red blood cell production.",
                Price = 14.99m,
                StockQuantity = 140,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FFFFD2/000000?text=Iron",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "B-Complex Vitamins",
                Description = "Energy and metabolism support with all 8 B vitamins.",
                Price = 16.99m,
                StockQuantity = 190,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/A8D8EA/FFFFFF?text=B-Complex",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Probiotics 10 Billion CFU",
                Description = "Digestive health support with 10 probiotic strains. Refrigerated formula.",
                Price = 32.99m,
                StockQuantity = 100,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FFAAA5/FFFFFF?text=Probiotics",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Glucosamine Chondroitin MSM",
                Description = "Joint health support for flexibility and mobility.",
                Price = 34.99m,
                StockQuantity = 110,
                CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FF8B94/FFFFFF?text=Glucosamine",
                WishlistCount = 0
            }
        ]);

        // Pain Relief (CategoryId = 2)
        products.AddRange(
        [
            new ProductModel
            {
                Id = productId++,
                Name = "Ibuprofen 400mg",
                Description = "Fast-acting pain and fever relief. Anti-inflammatory properties.",
                Price = 8.99m,
                StockQuantity = 300,
                CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/FFA07A/FFFFFF?text=Ibuprofen",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Aspirin 500mg",
                Description = "Pain relief and blood thinner. Reduces risk of heart attack.",
                Price = 6.99m,
                StockQuantity = 350,
                CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/98D8C8/FFFFFF?text=Aspirin",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Paracetamol 500mg",
                Description = "Effective pain and fever relief. Gentle on stomach.",
                Price = 7.99m,
                StockQuantity = 400,
                CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/F7B731/FFFFFF?text=Paracetamol",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Naproxen 250mg",
                Description = "Long-lasting pain relief up to 12 hours.",
                Price = 11.99m,
                StockQuantity = 200,
                CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/5F27CD/FFFFFF?text=Naproxen",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Muscle Pain Relief Gel",
                Description = "Topical pain relief for muscles and joints. Fast absorption.",
                Price = 13.99m,
                StockQuantity = 180,
                CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/00D2FF/FFFFFF?text=Pain+Gel",
                WishlistCount = 0
            }
        ]);

        // Cold & Flu (CategoryId = 3)
        products.AddRange(
        [
            new ProductModel
            {
                Id = productId++,
                Name = "Cold & Flu Relief Day & Night",
                Description = "Multi-symptom cold relief. Daytime and nighttime formulas.",
                Price = 12.99m,
                StockQuantity = 250,
                CategoryId = 3,
                ImageUrl = "https://via.placeholder.com/400/1E90FF/FFFFFF?text=Cold+Relief",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Cough Syrup",
                Description = "Soothes cough and throat irritation. Non-drowsy formula.",
                Price = 10.99m,
                StockQuantity = 200,
                CategoryId = 3,
                ImageUrl = "https://via.placeholder.com/400/ED4C67/FFFFFF?text=Cough+Syrup",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Throat Lozenges",
                Description = "Soothing throat relief with menthol and honey.",
                Price = 5.99m,
                StockQuantity = 300,
                CategoryId = 3,
                ImageUrl = "https://via.placeholder.com/400/B33771/FFFFFF?text=Lozenges",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Nasal Decongestant Spray",
                Description = "Fast nasal congestion relief. Works in minutes.",
                Price = 9.99m,
                StockQuantity = 180,
                CategoryId = 3,
                ImageUrl = "https://via.placeholder.com/400/3B3B98/FFFFFF?text=Nasal+Spray",
                WishlistCount = 0
            }
        ]);

        // First Aid (CategoryId = 4)
        products.AddRange(
        [
            new ProductModel
            {
                Id = productId++,
                Name = "Adhesive Bandages 100pk",
                Description = "Assorted sizes bandages. Flexible and breathable.",
                Price = 6.99m,
                StockQuantity = 500,
                CategoryId = 4,
                ImageUrl = "https://via.placeholder.com/400/26de81/FFFFFF?text=Bandages",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Sterile Gauze Pads",
                Description = "Medical grade gauze for wound care. Highly absorbent.",
                Price = 8.99m,
                StockQuantity = 300,
                CategoryId = 4,
                ImageUrl = "https://via.placeholder.com/400/20bf6b/FFFFFF?text=Gauze",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Antiseptic Solution 500ml",
                Description = "Wound cleansing solution. Prevents infection.",
                Price = 7.99m,
                StockQuantity = 250,
                CategoryId = 4,
                ImageUrl = "https://via.placeholder.com/400/45aaf2/FFFFFF?text=Antiseptic",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "First Aid Kit Deluxe",
                Description = "Complete first aid kit with 100+ items for home or travel.",
                Price = 34.99m,
                StockQuantity = 80,
                CategoryId = 4,
                ImageUrl = "https://via.placeholder.com/400/ff9ff3/000000?text=First+Aid+Kit",
                WishlistCount = 0
            }
        ]);

        // Skincare (CategoryId = 5)
        products.AddRange(
        [
            new ProductModel
            {
                Id = productId++,
                Name = "Moisturizing Cream",
                Description = "Deep hydration for all skin types. Non-greasy formula.",
                Price = 16.99m,
                StockQuantity = 180,
                CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/feca57/000000?text=Moisturizer",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Acne Treatment Gel",
                Description = "Clears acne and prevents breakouts. Contains benzoyl peroxide.",
                Price = 19.99m,
                StockQuantity = 140,
                CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/48dbfb/FFFFFF?text=Acne+Gel",
                WishlistCount = 0
            },
            new ProductModel
            {
                Id = productId++,
                Name = "Sunscreen SPF 50+",
                Description = "Broad spectrum sun protection. Water resistant 80 minutes.",
                Price = 14.99m,
                StockQuantity = 220,
                CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/ff9ff3/FFFFFF?text=Sunscreen",
                WishlistCount = 0
            }
        ]);
        return products;
    }
}
