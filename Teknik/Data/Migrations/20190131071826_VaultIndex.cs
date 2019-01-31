using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class VaultIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "VaultItems",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "VaultItems");
        }
    }
}
