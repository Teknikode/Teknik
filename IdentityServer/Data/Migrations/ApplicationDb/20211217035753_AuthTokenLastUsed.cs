using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.IdentityServer.Data.Migrations.ApplicationDb
{
    public partial class AuthTokenLastUsed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsed",
                table: "AuthTokens",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUsed",
                table: "AuthTokens");
        }
    }
}
