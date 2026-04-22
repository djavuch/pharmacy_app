using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Products"
                SET "ProductCode" = 'PRD-' || LPAD("Id"::text, 6, '0')
                WHERE "ProductCode" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "Products");
        }
    }
}
