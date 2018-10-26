using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class RemovedVerificationTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecoveryEmailVerifications");

            migrationBuilder.DropTable(
                name: "ResetPasswordVerifications");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecoveryEmailVerifications",
                columns: table => new
                {
                    RecoveryEmailVerificationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryEmailVerifications", x => x.RecoveryEmailVerificationId);
                    table.ForeignKey(
                        name: "FK_RecoveryEmailVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResetPasswordVerifications",
                columns: table => new
                {
                    ResetPasswordVerificationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswordVerifications", x => x.ResetPasswordVerificationId);
                    table.ForeignKey(
                        name: "FK_ResetPasswordVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecoveryEmailVerifications_UserId",
                table: "RecoveryEmailVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordVerifications_UserId",
                table: "ResetPasswordVerifications",
                column: "UserId");
        }
    }
}
