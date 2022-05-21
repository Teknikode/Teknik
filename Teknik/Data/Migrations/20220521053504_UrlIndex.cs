using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class UrlIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vaults_Url",
                table: "Vaults",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_Url",
                table: "Uploads",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrls_ShortUrl",
                table: "ShortenedUrls",
                column: "ShortUrl");

            migrationBuilder.CreateIndex(
                name: "IX_Pastes_Url",
                table: "Pastes",
                column: "Url");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vaults_Url",
                table: "Vaults");

            migrationBuilder.DropIndex(
                name: "IX_Uploads_Url",
                table: "Uploads");

            migrationBuilder.DropIndex(
                name: "IX_ShortenedUrls_ShortUrl",
                table: "ShortenedUrls");

            migrationBuilder.DropIndex(
                name: "IX_Pastes_Url",
                table: "Pastes");
        }
    }
}
