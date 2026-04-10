using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 27, 5, "Reduces wrinkles and fine lines. Contains retinol and hyaluronic acid.", "https://via.placeholder.com/400/f9ca24/000000?text=Serum", "Anti-Aging Serum", 39.99m, 90 },
                    { 28, 5, "Soothes and relieves dry, itchy eczema-prone skin. Fragrance-free.", "https://via.placeholder.com/400/6ab04c/FFFFFF?text=Eczema+Cream", "Eczema Relief Cream", 22.99m, 120 },
                    { 29, 6, "Fast relief from heartburn and acid indigestion. Chewable formula.", "https://via.placeholder.com/400/6c5ce7/FFFFFF?text=Antacid", "Antacid Tablets", 9.99m, 280 },
                    { 30, 6, "Supports digestion of proteins, fats and carbohydrates. Take with meals.", "https://via.placeholder.com/400/00b894/FFFFFF?text=Enzymes", "Digestive Enzymes", 27.99m, 130 },
                    { 31, 6, "Gentle overnight relief from occasional constipation.", "https://via.placeholder.com/400/fdcb6e/000000?text=Laxative", "Laxative Tablets", 8.49m, 200 },
                    { 32, 6, "Controls symptoms of diarrhea quickly and effectively.", "https://via.placeholder.com/400/e17055/FFFFFF?text=Anti-Diarrheal", "Anti-Diarrheal Capsules", 11.49m, 170 },
                    { 33, 6, "Daily fiber intake support. Improves gut motility and stool regularity.", "https://via.placeholder.com/400/55efc4/000000?text=Fiber", "Fiber Supplement 300g", 18.99m, 150 },
                    { 34, 7, "Essential vitamin D for infants. 400 IU per drop. Unflavored.", "https://via.placeholder.com/400/fdcb6e/000000?text=Baby+Vit+D", "Baby Vitamin D Drops", 14.99m, 160 },
                    { 35, 7, "Paracetamol suspension for infants. Strawberry flavour. 100ml.", "https://via.placeholder.com/400/fd79a8/FFFFFF?text=Baby+Fever", "Infant Fever Reducer", 9.99m, 220 },
                    { 36, 7, "Gentle nasal aspirator for newborns and infants. Easy to clean.", "https://via.placeholder.com/400/74b9ff/FFFFFF?text=Aspirator", "Baby Nasal Aspirator", 12.99m, 140 },
                    { 37, 7, "Soothes and protects sensitive baby skin. Zinc oxide formula.", "https://via.placeholder.com/400/a29bfe/FFFFFF?text=Diaper+Cream", "Diaper Rash Cream", 8.99m, 260 },
                    { 38, 7, "Supports healthy gut microbiome in infants. No artificial additives.", "https://via.placeholder.com/400/00cec9/FFFFFF?text=Baby+Probiotic", "Baby Probiotics Drops", 21.99m, 100 },
                    { 39, 8, "Upper arm blood pressure monitor with memory for 60 readings. WHO indicator.", "https://via.placeholder.com/400/2d3436/FFFFFF?text=BP+Monitor", "Digital Blood Pressure Monitor", 49.99m, 70 },
                    { 40, 8, "Fast 10-second reading. Fever alert. Suitable for all ages.", "https://via.placeholder.com/400/636e72/FFFFFF?text=Thermometer", "Digital Thermometer", 12.99m, 200 },
                    { 41, 8, "Measures blood oxygen saturation and pulse rate. Portable design.", "https://via.placeholder.com/400/b2bec3/000000?text=Oximeter", "Pulse Oximeter", 29.99m, 90 },
                    { 42, 8, "Converts liquid medication into fine mist for inhalation. Quiet operation.", "https://via.placeholder.com/400/dfe6e9/000000?text=Nebulizer", "Nebulizer Machine", 59.99m, 50 },
                    { 43, 8, "Compact wrist monitor with irregular heartbeat detection.", "https://via.placeholder.com/400/0984e3/FFFFFF?text=Wrist+BP", "Wrist Blood Pressure Monitor", 39.99m, 80 },
                    { 44, 9, "Instant relief for dry and irritated eyes. Preservative-free.", "https://via.placeholder.com/400/0652DD/FFFFFF?text=Eye+Drops", "Lubricating Eye Drops", 10.99m, 240 },
                    { 45, 9, "Relieves redness and itching due to allergies. Fast acting.", "https://via.placeholder.com/400/1289A7/FFFFFF?text=Allergy+Drops", "Antihistamine Eye Drops", 13.99m, 180 },
                    { 46, 9, "All-in-one solution for cleaning, rinsing and storing contact lenses.", "https://via.placeholder.com/400/C4E538/000000?text=Lens+Solution", "Contact Lens Solution 360ml", 11.99m, 200 },
                    { 47, 9, "Lutein and zeaxanthin formula to support macular health and vision.", "https://via.placeholder.com/400/FDA7DF/000000?text=Eye+Vitamins", "Eye Vitamins Complex", 24.99m, 130 },
                    { 48, 10, "Advanced whitening formula with fluoride. Removes surface stains.", "https://via.placeholder.com/400/ffffff/000000?text=Toothpaste", "Whitening Toothpaste", 7.99m, 350 },
                    { 49, 10, "Kills 99.9% of bacteria. Freshens breath for 12 hours.", "https://via.placeholder.com/400/00b894/FFFFFF?text=Mouthwash", "Antibacterial Mouthwash 500ml", 9.99m, 280 },
                    { 50, 10, "Waxed dental floss for easy gliding between teeth. Mint flavored.", "https://via.placeholder.com/400/55efc4/000000?text=Dental+Floss", "Dental Floss 50m", 4.99m, 400 },
                    { 51, 10, "Relieves tooth sensitivity in 2 weeks. Enamel protection formula.", "https://via.placeholder.com/400/b2bec3/000000?text=Sensitive+TP", "Sensitive Toothpaste", 8.99m, 300 },
                    { 52, 10, "Compatible replacement heads with round bristle technology.", "https://via.placeholder.com/400/74b9ff/FFFFFF?text=Brush+Heads", "Electric Toothbrush Heads 4pk", 19.99m, 160 },
                    { 53, 11, "48-hour protection against sweat and odor. Alcohol-free formula.", "https://via.placeholder.com/400/a29bfe/FFFFFF?text=Deodorant", "Antiperspirant Deodorant", 6.99m, 320 },
                    { 54, 11, "Controls dandruff and seborrheic dermatitis. Contains ketoconazole.", "https://via.placeholder.com/400/6c5ce7/FFFFFF?text=Shampoo", "Medicated Shampoo", 14.99m, 180 },
                    { 55, 11, "70% alcohol gel. Kills 99.99% of germs without water.", "https://via.placeholder.com/400/00cec9/FFFFFF?text=Hand+Sanitizer", "Hand Sanitizer 250ml", 5.99m, 400 },
                    { 56, 11, "Intensive moisturizing cream for cracked heels and dry feet.", "https://via.placeholder.com/400/fd79a8/FFFFFF?text=Foot+Cream", "Foot Cream with Urea", 11.99m, 150 },
                    { 57, 12, "Complete prenatal formula with folic acid, iron and DHA for mother and baby.", "https://via.placeholder.com/400/fd79a8/FFFFFF?text=Prenatal", "Prenatal Vitamins", 29.99m, 140 },
                    { 58, 12, "Supports hormonal balance and relieves PMS symptoms.", "https://via.placeholder.com/400/e84393/FFFFFF?text=Primrose+Oil", "Evening Primrose Oil 1000mg", 22.99m, 120 },
                    { 59, 12, "Supports urinary tract health. High potency 36mg PAC per capsule.", "https://via.placeholder.com/400/d63031/FFFFFF?text=Cranberry", "Cranberry Extract Capsules", 18.99m, 160 },
                    { 60, 12, "Contains black cohosh and soy isoflavones to ease menopause symptoms.", "https://via.placeholder.com/400/b2bec3/000000?text=Menopause", "Menopause Support Formula", 34.99m, 90 },
                    { 61, 13, "Complete daily multivitamin formulated for men's specific nutritional needs.", "https://via.placeholder.com/400/0984e3/FFFFFF?text=Men+Multi", "Men's Multivitamin", 26.99m, 150 },
                    { 62, 13, "Supports prostate health and healthy urinary flow in men.", "https://via.placeholder.com/400/2d3436/FFFFFF?text=Saw+Palmetto", "Saw Palmetto 320mg", 21.99m, 130 },
                    { 63, 13, "Natural formula with zinc, vitamin D and ashwagandha to support healthy testosterone levels.", "https://via.placeholder.com/400/636e72/FFFFFF?text=Testo+Support", "Testosterone Support Complex", 38.99m, 100 },
                    { 64, 13, "Improves muscle strength and exercise performance. Unflavored powder.", "https://via.placeholder.com/400/00b894/FFFFFF?text=Creatine", "Creatine Monohydrate 250g", 24.99m, 170 },
                    { 65, 14, "Compatible with most glucose meters. Accurate results in 5 seconds.", "https://via.placeholder.com/400/fdcb6e/000000?text=Test+Strips", "Blood Glucose Test Strips 50pk", 22.99m, 200 },
                    { 66, 14, "Accurate blood glucose monitoring. Large display. Memory for 500 readings.", "https://via.placeholder.com/400/e17055/FFFFFF?text=Glucose+Meter", "Digital Glucose Meter", 34.99m, 80 },
                    { 67, 14, "Ultra-thin 30G lancets for nearly painless blood sampling.", "https://via.placeholder.com/400/d63031/FFFFFF?text=Lancets", "Lancets 100pk", 9.99m, 300 },
                    { 68, 14, "Intensive moisturizer for diabetic dry skin. Improves circulation.", "https://via.placeholder.com/400/b2bec3/000000?text=Diabetic+Cream", "Diabetic Foot Cream", 15.99m, 140 },
                    { 69, 14, "Antioxidant support for nerve health in diabetics. Reduces oxidative stress.", "https://via.placeholder.com/400/6c5ce7/FFFFFF?text=Alpha+Lipoic", "Alpha Lipoic Acid 600mg", 27.99m, 110 },
                    { 70, 15, "Supports heart muscle energy production. Antioxidant protection for cells.", "https://via.placeholder.com/400/d63031/FFFFFF?text=CoQ10", "CoQ10 200mg", 36.99m, 120 },
                    { 71, 15, "Supports healthy heart rhythm and blood pressure. High absorption form.", "https://via.placeholder.com/400/e17055/FFFFFF?text=Magnesium", "Magnesium Glycinate 400mg", 23.99m, 150 },
                    { 72, 15, "Naturally supports healthy cholesterol levels. Standardized extract.", "https://via.placeholder.com/400/d63031/FFFFFF?text=Red+Yeast+Rice", "Red Yeast Rice Extract", 29.99m, 100 },
                    { 73, 15, "Traditional herbal support for cardiovascular function and circulation.", "https://via.placeholder.com/400/e84393/FFFFFF?text=Hawthorn", "Hawthorn Berry 600mg", 18.99m, 130 },
                    { 74, 15, "High-potency fish oil with 900mg EPA+DHA per capsule. Supports heart health.", "https://via.placeholder.com/400/0984e3/FFFFFF?text=Omega-3+TS", "Omega-3 Triple Strength", 39.99m, 110 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Reviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Reviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
