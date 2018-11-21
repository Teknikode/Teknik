using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class PasteFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "Uploads",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxDownloads",
                table: "Uploads",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeleteKey",
                table: "Pastes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Pastes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "MaxDownloads",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "DeleteKey",
                table: "Pastes");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Pastes");
        }
    }
}
