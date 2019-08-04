using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.IdentityServer.Data.Migrations.IdentityServer.ConfigurationDb
{
    public partial class _2_5_1_ConfigDBUpdateMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NonEditable",
                table: "IdentityResources",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DeviceCodeLifetime",
                table: "Clients",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NonEditable",
                table: "Clients",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserCodeType",
                table: "Clients",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSsoLifetime",
                table: "Clients",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NonEditable",
                table: "ApiResources",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NonEditable",
                table: "IdentityResources");

            migrationBuilder.DropColumn(
                name: "DeviceCodeLifetime",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NonEditable",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UserCodeType",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UserSsoLifetime",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NonEditable",
                table: "ApiResources");
        }
    }
}
