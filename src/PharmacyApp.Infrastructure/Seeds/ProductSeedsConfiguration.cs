using PharmacyApp.Domain.Entities;

namespace PharmacyApp.Infrastructure.Seeds;

public static class ProductSeedsConfiguration
{
    public static IEnumerable<object> GetSeedData()
    {
        int productId = 1;

        // Vitamins & Supplements (CategoryId = 1)
        var vitamins = new object[]
        {
            new
            {
                Id = productId++, Name = "Vitamin C 1000mg",
                Description = "High-potency vitamin C tablets for immune support. Take 1 tablet daily with food.",
                Price = 15.99m, StockQuantity = 200, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FF6B6B/FFFFFF?text=Vitamin+C", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Vitamin D3 5000 IU",
                Description = "Essential vitamin D for bone health and immune function. Supports calcium absorption.",
                Price = 18.99m, StockQuantity = 150, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/4ECDC4/FFFFFF?text=Vitamin+D3", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Multivitamin Complex",
                Description = "Daily multivitamin with essential nutrients for overall health and wellness.",
                Price = 24.99m, StockQuantity = 180, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/95E1D3/FFFFFF?text=Multivitamin", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Omega-3 Fish Oil 1200mg",
                Description = "High-quality omega-3 fatty acids for heart and brain health.", Price = 29.99m,
                StockQuantity = 120, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/F38181/FFFFFF?text=Omega-3", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Calcium + Magnesium",
                Description = "Bone support formula with vitamin D3 for optimal absorption.", Price = 19.99m,
                StockQuantity = 160, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/AA96DA/FFFFFF?text=Calcium", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Zinc 50mg",
                Description = "Immune system support and wound healing. Antioxidant properties.", Price = 12.99m,
                StockQuantity = 220, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FCBAD3/FFFFFF?text=Zinc", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Iron Supplement 65mg",
                Description = "Gentle iron formula for energy and red blood cell production.", Price = 14.99m,
                StockQuantity = 140, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FFFFD2/000000?text=Iron", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "B-Complex Vitamins",
                Description = "Energy and metabolism support with all 8 B vitamins.", Price = 16.99m,
                StockQuantity = 190, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/A8D8EA/FFFFFF?text=B-Complex", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Probiotics 10 Billion CFU",
                Description = "Digestive health support with 10 probiotic strains. Refrigerated formula.",
                Price = 32.99m, StockQuantity = 100, CategoryId = 1,
                ImageUrl = "https://via.placeholder.com/400/FFAAA5/FFFFFF?text=Probiotics", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Glucosamine Chondroitin MSM",
                Description = "Joint health support for flexibility and mobility.", Price = 34.99m, StockQuantity = 110,
                CategoryId = 1, ImageUrl = "https://via.placeholder.com/400/FF8B94/FFFFFF?text=Glucosamine",
                WishlistCount = 0
            },
        };

        // Pain Relief (CategoryId = 2)
        var painRelief = new object[]
        {
            new
            {
                Id = productId++, Name = "Ibuprofen 400mg",
                Description = "Fast-acting pain and fever relief. Anti-inflammatory properties.", Price = 8.99m,
                StockQuantity = 300, CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/FFA07A/FFFFFF?text=Ibuprofen", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Aspirin 500mg",
                Description = "Pain relief and blood thinner. Reduces risk of heart attack.", Price = 6.99m,
                StockQuantity = 350, CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/98D8C8/FFFFFF?text=Aspirin", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Paracetamol 500mg",
                Description = "Effective pain and fever relief. Gentle on stomach.", Price = 7.99m, StockQuantity = 400,
                CategoryId = 2, ImageUrl = "https://via.placeholder.com/400/F7B731/FFFFFF?text=Paracetamol",
                WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Naproxen 250mg",
                Description = "Long-lasting pain relief up to 12 hours.",
                Price = 11.99m, StockQuantity = 200, CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/5F27CD/FFFFFF?text=Naproxen", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Muscle Pain Relief Gel",
                Description = "Topical pain relief for muscles and joints. Fast absorption.", Price = 13.99m,
                StockQuantity = 180, CategoryId = 2,
                ImageUrl = "https://via.placeholder.com/400/00D2FF/FFFFFF?text=Pain+Gel", WishlistCount = 0
            },
        };

        // Cold & Flu (CategoryId = 3)
        var coldFlu = new object[]
        {
            new
            {
                Id = productId++, Name = "Cold & Flu Relief Day & Night",
                Description = "Multi-symptom cold relief. Daytime and nighttime formulas.", Price = 12.99m,
                StockQuantity = 250, CategoryId = 3,
                ImageUrl = "https://via.placeholder.com/400/1E90FF/FFFFFF?text=Cold+Relief", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Cough Syrup",
                Description = "Soothes cough and throat irritation. Non-drowsy formula.", Price = 10.99m,
                StockQuantity = 200, CategoryId = 3,
                ImageUrl = "https://via.placeholder.com/400/ED4C67/FFFFFF?text=Cough+Syrup", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Throat Lozenges",
                Description = "Soothing throat relief with menthol and honey.", Price = 5.99m, StockQuantity = 300,
                CategoryId = 3, ImageUrl = "https://via.placeholder.com/400/B33771/FFFFFF?text=Lozenges",
                WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Nasal Decongestant Spray",
                Description = "Fast nasal congestion relief. Works in minutes.", Price = 9.99m, StockQuantity = 180,
                CategoryId = 3, ImageUrl = "https://via.placeholder.com/400/3B3B98/FFFFFF?text=Nasal+Spray",
                WishlistCount = 0
            },
        };

        // First Aid (CategoryId = 4)
        var firstAid = new object[]
        {
            new
            {
                Id = productId++, Name = "Adhesive Bandages 100pk",
                Description = "Assorted sizes bandages. Flexible and breathable.", Price = 6.99m, StockQuantity = 500,
                CategoryId = 4, ImageUrl = "https://via.placeholder.com/400/26de81/FFFFFF?text=Bandages",
                WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Sterile Gauze Pads",
                Description = "Medical grade gauze for wound care. Highly absorbent.", Price = 8.99m,
                StockQuantity = 300, CategoryId = 4,
                ImageUrl = "https://via.placeholder.com/400/20bf6b/FFFFFF?text=Gauze", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Antiseptic Solution 500ml",
                Description = "Wound cleansing solution. Prevents infection.", Price = 7.99m, StockQuantity = 250,
                CategoryId = 4, ImageUrl = "https://via.placeholder.com/400/45aaf2/FFFFFF?text=Antiseptic",
                WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "First Aid Kit Deluxe",
                Description = "Complete first aid kit with 100+ items for home or travel.", Price = 34.99m,
                StockQuantity = 80, CategoryId = 4,
                ImageUrl = "https://via.placeholder.com/400/ff9ff3/000000?text=First+Aid+Kit", WishlistCount = 0
            },
        };

        // Skincare (CategoryId = 5)
        var skincare = new object[]
        {
            new
            {
                Id = productId++, Name = "Moisturizing Cream",
                Description = "Deep hydration for all skin types. Non-greasy formula.", Price = 16.99m,
                StockQuantity = 180, CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/feca57/000000?text=Moisturizer", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Acne Treatment Gel",
                Description = "Clears acne and prevents breakouts. Contains benzoyl peroxide.", Price = 19.99m,
                StockQuantity = 140, CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/48dbfb/FFFFFF?text=Acne+Gel", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Sunscreen SPF 50+",
                Description = "Broad spectrum sun protection. Water resistant 80 minutes.", Price = 14.99m,
                StockQuantity = 220, CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/ff9ff3/FFFFFF?text=Sunscreen", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Anti-Aging Serum",
                Description = "Reduces wrinkles and fine lines. Contains retinol and hyaluronic acid.", Price = 39.99m,
                StockQuantity = 90, CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/f9ca24/000000?text=Serum", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Eczema Relief Cream",
                Description = "Soothes and relieves dry, itchy eczema-prone skin. Fragrance-free.", Price = 22.99m,
                StockQuantity = 120, CategoryId = 5,
                ImageUrl = "https://via.placeholder.com/400/6ab04c/FFFFFF?text=Eczema+Cream", WishlistCount = 0
            },
        };

        // Digestive Health (CategoryId = 6)
        var digestive = new object[]
        {
            new
            {
                Id = productId++, Name = "Antacid Tablets",
                Description = "Fast relief from heartburn and acid indigestion. Chewable formula.", Price = 9.99m,
                StockQuantity = 280, CategoryId = 6,
                ImageUrl = "https://via.placeholder.com/400/6c5ce7/FFFFFF?text=Antacid", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Digestive Enzymes",
                Description = "Supports digestion of proteins, fats and carbohydrates. Take with meals.", Price = 27.99m,
                StockQuantity = 130, CategoryId = 6,
                ImageUrl = "https://via.placeholder.com/400/00b894/FFFFFF?text=Enzymes", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Laxative Tablets",
                Description = "Gentle overnight relief from occasional constipation.", Price = 8.49m,
                StockQuantity = 200, CategoryId = 6,
                ImageUrl = "https://via.placeholder.com/400/fdcb6e/000000?text=Laxative", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Anti-Diarrheal Capsules",
                Description = "Controls symptoms of diarrhea quickly and effectively.", Price = 11.49m,
                StockQuantity = 170, CategoryId = 6,
                ImageUrl = "https://via.placeholder.com/400/e17055/FFFFFF?text=Anti-Diarrheal", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Fiber Supplement 300g",
                Description = "Daily fiber intake support. Improves gut motility and stool regularity.", Price = 18.99m,
                StockQuantity = 150, CategoryId = 6,
                ImageUrl = "https://via.placeholder.com/400/55efc4/000000?text=Fiber", WishlistCount = 0
            },
        };

        // Baby Care (CategoryId = 7)
        var babyCare = new object[]
        {
            new
            {
                Id = productId++, Name = "Baby Vitamin D Drops",
                Description = "Essential vitamin D for infants. 400 IU per drop. Unflavored.", Price = 14.99m,
                StockQuantity = 160, CategoryId = 7,
                ImageUrl = "https://via.placeholder.com/400/fdcb6e/000000?text=Baby+Vit+D", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Infant Fever Reducer",
                Description = "Paracetamol suspension for infants. Strawberry flavour. 100ml.", Price = 9.99m,
                StockQuantity = 220, CategoryId = 7,
                ImageUrl = "https://via.placeholder.com/400/fd79a8/FFFFFF?text=Baby+Fever", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Baby Nasal Aspirator",
                Description = "Gentle nasal aspirator for newborns and infants. Easy to clean.", Price = 12.99m,
                StockQuantity = 140, CategoryId = 7,
                ImageUrl = "https://via.placeholder.com/400/74b9ff/FFFFFF?text=Aspirator", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Diaper Rash Cream",
                Description = "Soothes and protects sensitive baby skin. Zinc oxide formula.", Price = 8.99m,
                StockQuantity = 260, CategoryId = 7,
                ImageUrl = "https://via.placeholder.com/400/a29bfe/FFFFFF?text=Diaper+Cream", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Baby Probiotics Drops",
                Description = "Supports healthy gut microbiome in infants. No artificial additives.", Price = 21.99m,
                StockQuantity = 100, CategoryId = 7,
                ImageUrl = "https://via.placeholder.com/400/00cec9/FFFFFF?text=Baby+Probiotic", WishlistCount = 0
            },
        };

        // Medical Devices (CategoryId = 8)
        var medicalDevices = new object[]
        {
            new
            {
                Id = productId++, Name = "Digital Blood Pressure Monitor",
                Description = "Upper arm blood pressure monitor with memory for 60 readings. WHO indicator.", Price = 49.99m,
                StockQuantity = 70, CategoryId = 8,
                ImageUrl = "https://via.placeholder.com/400/2d3436/FFFFFF?text=BP+Monitor", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Digital Thermometer",
                Description = "Fast 10-second reading. Fever alert. Suitable for all ages.", Price = 12.99m,
                StockQuantity = 200, CategoryId = 8,
                ImageUrl = "https://via.placeholder.com/400/636e72/FFFFFF?text=Thermometer", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Pulse Oximeter",
                Description = "Measures blood oxygen saturation and pulse rate. Portable design.", Price = 29.99m,
                StockQuantity = 90, CategoryId = 8,
                ImageUrl = "https://via.placeholder.com/400/b2bec3/000000?text=Oximeter", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Nebulizer Machine",
                Description = "Converts liquid medication into fine mist for inhalation. Quiet operation.", Price = 59.99m,
                StockQuantity = 50, CategoryId = 8,
                ImageUrl = "https://via.placeholder.com/400/dfe6e9/000000?text=Nebulizer", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Wrist Blood Pressure Monitor",
                Description = "Compact wrist monitor with irregular heartbeat detection.", Price = 39.99m,
                StockQuantity = 80, CategoryId = 8,
                ImageUrl = "https://via.placeholder.com/400/0984e3/FFFFFF?text=Wrist+BP", WishlistCount = 0
            },
        };

        // Eye Care (CategoryId = 9)
        var eyeCare = new object[]
        {
            new
            {
                Id = productId++, Name = "Lubricating Eye Drops",
                Description = "Instant relief for dry and irritated eyes. Preservative-free.", Price = 10.99m,
                StockQuantity = 240, CategoryId = 9,
                ImageUrl = "https://via.placeholder.com/400/0652DD/FFFFFF?text=Eye+Drops", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Antihistamine Eye Drops",
                Description = "Relieves redness and itching due to allergies. Fast acting.", Price = 13.99m,
                StockQuantity = 180, CategoryId = 9,
                ImageUrl = "https://via.placeholder.com/400/1289A7/FFFFFF?text=Allergy+Drops", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Contact Lens Solution 360ml",
                Description = "All-in-one solution for cleaning, rinsing and storing contact lenses.", Price = 11.99m,
                StockQuantity = 200, CategoryId = 9,
                ImageUrl = "https://via.placeholder.com/400/C4E538/000000?text=Lens+Solution", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Eye Vitamins Complex",
                Description = "Lutein and zeaxanthin formula to support macular health and vision.", Price = 24.99m,
                StockQuantity = 130, CategoryId = 9,
                ImageUrl = "https://via.placeholder.com/400/FDA7DF/000000?text=Eye+Vitamins", WishlistCount = 0
            },
        };

        // Oral Care (CategoryId = 10)
        var oralCare = new object[]
        {
            new
            {
                Id = productId++, Name = "Whitening Toothpaste",
                Description = "Advanced whitening formula with fluoride. Removes surface stains.", Price = 7.99m,
                StockQuantity = 350, CategoryId = 10,
                ImageUrl = "https://via.placeholder.com/400/ffffff/000000?text=Toothpaste", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Antibacterial Mouthwash 500ml",
                Description = "Kills 99.9% of bacteria. Freshens breath for 12 hours.", Price = 9.99m,
                StockQuantity = 280, CategoryId = 10,
                ImageUrl = "https://via.placeholder.com/400/00b894/FFFFFF?text=Mouthwash", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Dental Floss 50m",
                Description = "Waxed dental floss for easy gliding between teeth. Mint flavored.", Price = 4.99m,
                StockQuantity = 400, CategoryId = 10,
                ImageUrl = "https://via.placeholder.com/400/55efc4/000000?text=Dental+Floss", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Sensitive Toothpaste",
                Description = "Relieves tooth sensitivity in 2 weeks. Enamel protection formula.", Price = 8.99m,
                StockQuantity = 300, CategoryId = 10,
                ImageUrl = "https://via.placeholder.com/400/b2bec3/000000?text=Sensitive+TP", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Electric Toothbrush Heads 4pk",
                Description = "Compatible replacement heads with round bristle technology.", Price = 19.99m,
                StockQuantity = 160, CategoryId = 10,
                ImageUrl = "https://via.placeholder.com/400/74b9ff/FFFFFF?text=Brush+Heads", WishlistCount = 0
            },
        };

        // Personal Care (CategoryId = 11)
        var personalCare = new object[]
        {
            new
            {
                Id = productId++, Name = "Antiperspirant Deodorant",
                Description = "48-hour protection against sweat and odor. Alcohol-free formula.", Price = 6.99m,
                StockQuantity = 320, CategoryId = 11,
                ImageUrl = "https://via.placeholder.com/400/a29bfe/FFFFFF?text=Deodorant", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Medicated Shampoo",
                Description = "Controls dandruff and seborrheic dermatitis. Contains ketoconazole.", Price = 14.99m,
                StockQuantity = 180, CategoryId = 11,
                ImageUrl = "https://via.placeholder.com/400/6c5ce7/FFFFFF?text=Shampoo", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Hand Sanitizer 250ml",
                Description = "70% alcohol gel. Kills 99.99% of germs without water.", Price = 5.99m,
                StockQuantity = 400, CategoryId = 11,
                ImageUrl = "https://via.placeholder.com/400/00cec9/FFFFFF?text=Hand+Sanitizer", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Foot Cream with Urea",
                Description = "Intensive moisturizing cream for cracked heels and dry feet.", Price = 11.99m,
                StockQuantity = 150, CategoryId = 11,
                ImageUrl = "https://via.placeholder.com/400/fd79a8/FFFFFF?text=Foot+Cream", WishlistCount = 0
            },
        };

        // Women's Health (CategoryId = 12)
        var womensHealth = new object[]
        {
            new
            {
                Id = productId++, Name = "Prenatal Vitamins",
                Description = "Complete prenatal formula with folic acid, iron and DHA for mother and baby.", Price = 29.99m,
                StockQuantity = 140, CategoryId = 12,
                ImageUrl = "https://via.placeholder.com/400/fd79a8/FFFFFF?text=Prenatal", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Evening Primrose Oil 1000mg",
                Description = "Supports hormonal balance and relieves PMS symptoms.", Price = 22.99m,
                StockQuantity = 120, CategoryId = 12,
                ImageUrl = "https://via.placeholder.com/400/e84393/FFFFFF?text=Primrose+Oil", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Cranberry Extract Capsules",
                Description = "Supports urinary tract health. High potency 36mg PAC per capsule.", Price = 18.99m,
                StockQuantity = 160, CategoryId = 12,
                ImageUrl = "https://via.placeholder.com/400/d63031/FFFFFF?text=Cranberry", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Menopause Support Formula",
                Description = "Contains black cohosh and soy isoflavones to ease menopause symptoms.", Price = 34.99m,
                StockQuantity = 90, CategoryId = 12,
                ImageUrl = "https://via.placeholder.com/400/b2bec3/000000?text=Menopause", WishlistCount = 0
            },
        };

        // Men's Health (CategoryId = 13)
        var mensHealth = new object[]
        {
            new
            {
                Id = productId++, Name = "Men's Multivitamin",
                Description = "Complete daily multivitamin formulated for men's specific nutritional needs.", Price = 26.99m,
                StockQuantity = 150, CategoryId = 13,
                ImageUrl = "https://via.placeholder.com/400/0984e3/FFFFFF?text=Men+Multi", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Saw Palmetto 320mg",
                Description = "Supports prostate health and healthy urinary flow in men.", Price = 21.99m,
                StockQuantity = 130, CategoryId = 13,
                ImageUrl = "https://via.placeholder.com/400/2d3436/FFFFFF?text=Saw+Palmetto", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Testosterone Support Complex",
                Description = "Natural formula with zinc, vitamin D and ashwagandha to support healthy testosterone levels.", Price = 38.99m,
                StockQuantity = 100, CategoryId = 13,
                ImageUrl = "https://via.placeholder.com/400/636e72/FFFFFF?text=Testo+Support", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Creatine Monohydrate 250g",
                Description = "Improves muscle strength and exercise performance. Unflavored powder.", Price = 24.99m,
                StockQuantity = 170, CategoryId = 13,
                ImageUrl = "https://via.placeholder.com/400/00b894/FFFFFF?text=Creatine", WishlistCount = 0
            },
        };

        // Diabetes Care (CategoryId = 14)
        var diabetesCare = new object[]
        {
            new
            {
                Id = productId++, Name = "Blood Glucose Test Strips 50pk",
                Description = "Compatible with most glucose meters. Accurate results in 5 seconds.", Price = 22.99m,
                StockQuantity = 200, CategoryId = 14,
                ImageUrl = "https://via.placeholder.com/400/fdcb6e/000000?text=Test+Strips", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Digital Glucose Meter",
                Description = "Accurate blood glucose monitoring. Large display. Memory for 500 readings.", Price = 34.99m,
                StockQuantity = 80, CategoryId = 14,
                ImageUrl = "https://via.placeholder.com/400/e17055/FFFFFF?text=Glucose+Meter", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Lancets 100pk",
                Description = "Ultra-thin 30G lancets for nearly painless blood sampling.", Price = 9.99m,
                StockQuantity = 300, CategoryId = 14,
                ImageUrl = "https://via.placeholder.com/400/d63031/FFFFFF?text=Lancets", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Diabetic Foot Cream",
                Description = "Intensive moisturizer for diabetic dry skin. Improves circulation.", Price = 15.99m,
                StockQuantity = 140, CategoryId = 14,
                ImageUrl = "https://via.placeholder.com/400/b2bec3/000000?text=Diabetic+Cream", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Alpha Lipoic Acid 600mg",
                Description = "Antioxidant support for nerve health in diabetics. Reduces oxidative stress.", Price = 27.99m,
                StockQuantity = 110, CategoryId = 14,
                ImageUrl = "https://via.placeholder.com/400/6c5ce7/FFFFFF?text=Alpha+Lipoic", WishlistCount = 0
            },
        };

        // Heart Health (CategoryId = 15)
        var heartHealth = new object[]
        {
            new
            {
                Id = productId++, Name = "CoQ10 200mg",
                Description = "Supports heart muscle energy production. Antioxidant protection for cells.", Price = 36.99m,
                StockQuantity = 120, CategoryId = 15,
                ImageUrl = "https://via.placeholder.com/400/d63031/FFFFFF?text=CoQ10", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Magnesium Glycinate 400mg",
                Description = "Supports healthy heart rhythm and blood pressure. High absorption form.", Price = 23.99m,
                StockQuantity = 150, CategoryId = 15,
                ImageUrl = "https://via.placeholder.com/400/e17055/FFFFFF?text=Magnesium", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Red Yeast Rice Extract",
                Description = "Naturally supports healthy cholesterol levels. Standardized extract.", Price = 29.99m,
                StockQuantity = 100, CategoryId = 15,
                ImageUrl = "https://via.placeholder.com/400/d63031/FFFFFF?text=Red+Yeast+Rice", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Hawthorn Berry 600mg",
                Description = "Traditional herbal support for cardiovascular function and circulation.", Price = 18.99m,
                StockQuantity = 130, CategoryId = 15,
                ImageUrl = "https://via.placeholder.com/400/e84393/FFFFFF?text=Hawthorn", WishlistCount = 0
            },
            new
            {
                Id = productId++, Name = "Omega-3 Triple Strength",
                Description = "High-potency fish oil with 900mg EPA+DHA per capsule. Supports heart health.", Price = 39.99m,
                StockQuantity = 110, CategoryId = 15,
                ImageUrl = "https://via.placeholder.com/400/0984e3/FFFFFF?text=Omega-3+TS", WishlistCount = 0
            },
        };

        return
        [
            ..vitamins, ..painRelief, ..coldFlu, ..firstAid, ..skincare,
            ..digestive, ..babyCare, ..medicalDevices, ..eyeCare, ..oralCare,
            ..personalCare, ..womensHealth, ..mensHealth, ..diabetesCare, ..heartHealth
        ];
    }
}