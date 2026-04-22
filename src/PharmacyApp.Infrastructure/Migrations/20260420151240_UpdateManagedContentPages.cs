using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManagedContentPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentPages",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Content", "Slug", "Title" },
                values: new object[] { "This is a starter license agreement page.\nReplace this text from the admin panel before production.", "license-agreement", "License Agreement" });

            migrationBuilder.InsertData(
                table: "ContentPages",
                columns: new[] { "Id", "Content", "CreatedAt", "IsPublished", "Slug", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 3, "Tell customers who you are, where you operate, and why they can trust your pharmacy.", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "about", "About Company", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentPages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "ContentPages",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Content", "Slug", "Title" },
                values: new object[] { "This is a starter privacy policy page.\nReplace this text from the admin panel before production.", "privacy", "Privacy Policy" });
        }
    }
}
