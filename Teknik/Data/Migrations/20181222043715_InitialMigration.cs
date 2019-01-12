using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Teknik.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    ContactId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.ContactId);
                });

            migrationBuilder.CreateTable(
                name: "Podcasts",
                columns: table => new
                {
                    PodcastId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Episode = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Published = table.Column<bool>(nullable: false),
                    DatePosted = table.Column<DateTime>(nullable: false),
                    DatePublished = table.Column<DateTime>(nullable: false),
                    DateEdited = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Podcasts", x => x.PodcastId);
                });

            migrationBuilder.CreateTable(
                name: "Takedowns",
                columns: table => new
                {
                    TakedownId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Requester = table.Column<string>(nullable: true),
                    RequesterContact = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    ActionTaken = table.Column<string>(nullable: true),
                    DateRequested = table.Column<DateTime>(nullable: false),
                    DateActionTaken = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Takedowns", x => x.TakedownId);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(type: "decimal(19, 5)", nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    DateSent = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(nullable: true),
                    About = table.Column<string>(nullable: true),
                    Website = table.Column<string>(nullable: true),
                    Quote = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Encrypt = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "PodcastFiles",
                columns: table => new
                {
                    PodcastFileId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PodcastId = table.Column<int>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ContentLength = table.Column<long>(nullable: false),
                    Size = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastFiles", x => x.PodcastFileId);
                    table.ForeignKey(
                        name: "FK_PodcastFiles_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PodcastTags",
                columns: table => new
                {
                    PodcastTagId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PodcastId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastTags", x => x.PodcastTagId);
                    table.ForeignKey(
                        name: "FK_PodcastTags_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogId);
                    table.ForeignKey(
                        name: "FK_Blogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InviteCodes",
                columns: table => new
                {
                    InviteCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    OwnerId = table.Column<int>(nullable: true),
                    ClaimedUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteCodes", x => x.InviteCodeId);
                    table.ForeignKey(
                        name: "FK_InviteCodes_Users_ClaimedUserId",
                        column: x => x.ClaimedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InviteCodes_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pastes",
                columns: table => new
                {
                    PasteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    DatePosted = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Syntax = table.Column<string>(nullable: true),
                    ExpireDate = table.Column<DateTime>(nullable: true),
                    HashedPassword = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    KeySize = table.Column<int>(nullable: false),
                    IV = table.Column<string>(nullable: true),
                    BlockSize = table.Column<int>(nullable: false),
                    DeleteKey = table.Column<string>(nullable: true),
                    Hide = table.Column<bool>(nullable: false),
                    MaxViews = table.Column<int>(nullable: false),
                    Views = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pastes", x => x.PasteId);
                    table.ForeignKey(
                        name: "FK_Pastes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PodcastComments",
                columns: table => new
                {
                    PodcastCommentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PodcastId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    DatePosted = table.Column<DateTime>(nullable: false),
                    DateEdited = table.Column<DateTime>(nullable: false),
                    Article = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastComments", x => x.PodcastCommentId);
                    table.ForeignKey(
                        name: "FK_PodcastComments_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PodcastComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShortenedUrls",
                columns: table => new
                {
                    ShortenedUrlId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    ShortUrl = table.Column<string>(nullable: true),
                    OriginalUrl = table.Column<string>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Views = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenedUrls", x => x.ShortenedUrlId);
                    table.ForeignKey(
                        name: "FK_ShortenedUrls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Uploads",
                columns: table => new
                {
                    UploadId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    DateUploaded = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    ContentLength = table.Column<long>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    IV = table.Column<string>(nullable: true),
                    KeySize = table.Column<int>(nullable: false),
                    BlockSize = table.Column<int>(nullable: false),
                    DeleteKey = table.Column<string>(nullable: true),
                    ExpireDate = table.Column<DateTime>(nullable: true),
                    MaxDownloads = table.Column<int>(nullable: false),
                    Downloads = table.Column<int>(nullable: false),
                    Takedown_TakedownId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploads", x => x.UploadId);
                    table.ForeignKey(
                        name: "FK_Uploads_Takedowns_Takedown_TakedownId",
                        column: x => x.Takedown_TakedownId,
                        principalTable: "Takedowns",
                        principalColumn: "TakedownId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Uploads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginInfoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LoginProvider = table.Column<string>(nullable: true),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    ProviderKey = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => x.LoginInfoId);
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vaults",
                columns: table => new
                {
                    VaultId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateEdited = table.Column<DateTime>(nullable: false),
                    Views = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaults", x => x.VaultId);
                    table.ForeignKey(
                        name: "FK_Vaults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    BlogPostId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlogId = table.Column<int>(nullable: false),
                    System = table.Column<bool>(nullable: false),
                    DatePosted = table.Column<DateTime>(nullable: false),
                    DatePublished = table.Column<DateTime>(nullable: false),
                    DateEdited = table.Column<DateTime>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Article = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.BlogPostId);
                    table.ForeignKey(
                        name: "FK_BlogPosts_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaultItems",
                columns: table => new
                {
                    VaultItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VaultId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    PasteId = table.Column<int>(nullable: true),
                    UploadId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultItems", x => x.VaultItemId);
                    table.ForeignKey(
                        name: "FK_VaultItems_Pastes_PasteId",
                        column: x => x.PasteId,
                        principalTable: "Pastes",
                        principalColumn: "PasteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaultItems_Uploads_UploadId",
                        column: x => x.UploadId,
                        principalTable: "Uploads",
                        principalColumn: "UploadId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaultItems_Vaults_VaultId",
                        column: x => x.VaultId,
                        principalTable: "Vaults",
                        principalColumn: "VaultId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPostComments",
                columns: table => new
                {
                    BlogPostCommentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlogPostId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    DatePosted = table.Column<DateTime>(nullable: false),
                    DateEdited = table.Column<DateTime>(nullable: false),
                    Article = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostComments", x => x.BlogPostCommentId);
                    table.ForeignKey(
                        name: "FK_BlogPostComments_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "BlogPostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogPostComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlogPostTags",
                columns: table => new
                {
                    BlogPostTagId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlogPostId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostTags", x => x.BlogPostTagId);
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "BlogPostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostComments_BlogPostId",
                table: "BlogPostComments",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostComments_UserId",
                table: "BlogPostComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_BlogId",
                table: "BlogPosts",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_BlogPostId",
                table: "BlogPostTags",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_UserId",
                table: "Blogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteCodes_ClaimedUserId",
                table: "InviteCodes",
                column: "ClaimedUserId",
                unique: true,
                filter: "[ClaimedUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InviteCodes_OwnerId",
                table: "InviteCodes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pastes_UserId",
                table: "Pastes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastComments_PodcastId",
                table: "PodcastComments",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastComments_UserId",
                table: "PodcastComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastFiles_PodcastId",
                table: "PodcastFiles",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastTags_PodcastId",
                table: "PodcastTags",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrls_UserId",
                table: "ShortenedUrls",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_Takedown_TakedownId",
                table: "Uploads",
                column: "Takedown_TakedownId");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_UserId",
                table: "Uploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultItems_PasteId",
                table: "VaultItems",
                column: "PasteId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultItems_UploadId",
                table: "VaultItems",
                column: "UploadId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultItems_VaultId",
                table: "VaultItems",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaults_UserId",
                table: "Vaults",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostComments");

            migrationBuilder.DropTable(
                name: "BlogPostTags");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "InviteCodes");

            migrationBuilder.DropTable(
                name: "PodcastComments");

            migrationBuilder.DropTable(
                name: "PodcastFiles");

            migrationBuilder.DropTable(
                name: "PodcastTags");

            migrationBuilder.DropTable(
                name: "ShortenedUrls");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "VaultItems");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "Podcasts");

            migrationBuilder.DropTable(
                name: "Pastes");

            migrationBuilder.DropTable(
                name: "Uploads");

            migrationBuilder.DropTable(
                name: "Vaults");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Takedowns");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
