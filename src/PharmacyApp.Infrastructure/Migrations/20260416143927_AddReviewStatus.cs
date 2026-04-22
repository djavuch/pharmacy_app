using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Reviews",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql("""
                UPDATE "Reviews"
                SET "Status" = 'Approved'
                WHERE "IsApproved" = TRUE;
                """);

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Reviews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "Reviews"
                SET "IsApproved" = TRUE
                WHERE "Status" = 'Approved';
                """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reviews");
        }
    }
}
