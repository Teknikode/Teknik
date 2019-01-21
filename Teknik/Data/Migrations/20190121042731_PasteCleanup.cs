using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class PasteCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hide",
                table: "Pastes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Hide",
                table: "Pastes",
                nullable: false,
                defaultValue: false);
        }
    }
}
