using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReviewsOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_Status",
                table: "Reviews",
                columns: new[] { "ProductId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_Status",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");
        }
    }
}
