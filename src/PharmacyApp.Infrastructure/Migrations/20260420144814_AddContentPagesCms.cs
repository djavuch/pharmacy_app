using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPagesCms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ContentPages",
                columns: new[] { "Id", "Content", "CreatedAt", "IsPublished", "Slug", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "This is a starter privacy policy page.\nReplace this text from the admin panel before production.", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "privacy", "Privacy Policy", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" },
                    { 2, "Email: support@pharmacyapp.local\nPhone: +1 (000) 000-0000\nAddress: Replace with your real support address.", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "contacts", "Contacts", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPages_Slug",
                table: "ContentPages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPages");
        }
    }
}
