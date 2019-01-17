using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class UploadExpirationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpireLength",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExpireUnit",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpireLength",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExpireUnit",
                table: "Users");
        }
    }
}
