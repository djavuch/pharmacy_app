using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserAddressFromIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }
    }
}
