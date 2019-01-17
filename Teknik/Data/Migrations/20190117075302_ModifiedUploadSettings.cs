using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class ModifiedUploadSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpireLength",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ExpireUnit",
                table: "Users",
                newName: "ExpirationLength");

            migrationBuilder.AddColumn<int>(
                name: "ExpirationUnit",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationUnit",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ExpirationLength",
                table: "Users",
                newName: "ExpireUnit");

            migrationBuilder.AddColumn<int>(
                name: "ExpireLength",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }
    }
}
