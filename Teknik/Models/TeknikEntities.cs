using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Contact.Models;
using Teknik.Migrations;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Paste.Models;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Stats.Models;
using Teknik.Areas.Shortener.Models;
using Teknik.Attributes;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Teknik.Areas.Vault.Models;

namespace Teknik.Models
{
    public class TeknikEntities : DbContext
    {
        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<TrustedDevice> TrustedDevices { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<TransferType> TransferTypes { get; set; }
        public DbSet<InviteCode> InviteCodes { get; set; }
        // User Settings
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<SecuritySettings> SecuritySettings { get; set; }
        public DbSet<BlogSettings> BlogSettings { get; set; }
        public DbSet<UploadSettings> UploadSettings { get; set; }
        // Authentication and Sessions
        public DbSet<RecoveryEmailVerification> RecoveryEmailVerifications { get; set; }
        public DbSet<ResetPasswordVerification> ResetPasswordVerifications { get; set; }
        // Blogs
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogPostComment> BlogComments { get; set; }
        // Contact
        public DbSet<Contact> Contact { get; set; }
        // Uploads
        public DbSet<Upload> Uploads { get; set; }
        // Pastes
        public DbSet<Paste> Pastes { get; set; }
        // Podcasts
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<PodcastFile> PodcastFiles { get; set; }
        public DbSet<PodcastComment> PodcastComments { get; set; }
        // Transparency
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Takedown> Takedowns { get; set; }
        // Url Shortener
        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
        // Vaults
        public DbSet<Vault> Vaults { get; set; }
        public DbSet<VaultItem> VaultItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // User Settings Mappings
            modelBuilder.Entity<User>()
                .HasRequired(a => a.UserSettings)
                .WithRequiredPrincipal(a => a.User);
            modelBuilder.Entity<User>()
                .HasRequired(a => a.SecuritySettings)
                .WithRequiredPrincipal(a => a.User);
            modelBuilder.Entity<User>()
                .HasRequired(a => a.BlogSettings)
                .WithRequiredPrincipal(a => a.User);
            modelBuilder.Entity<User>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.User);

            modelBuilder.Entity<UserSettings>()
                .HasRequired(a => a.SecuritySettings)
                .WithRequiredPrincipal(a => a.UserSettings);
            modelBuilder.Entity<UserSettings>()
                .HasRequired(a => a.BlogSettings)
                .WithRequiredPrincipal(a => a.UserSettings);
            modelBuilder.Entity<UserSettings>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.UserSettings);

            modelBuilder.Entity<SecuritySettings>()
                .HasRequired(a => a.BlogSettings)
                .WithRequiredPrincipal(a => a.SecuritySettings);
            modelBuilder.Entity<SecuritySettings>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.SecuritySettings);

            modelBuilder.Entity<BlogSettings>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.BlogSettings);

            // User Mappings
            modelBuilder.Entity<User>()
                .HasMany<Upload>(u => u.Uploads)
                .WithOptional(u => u.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany<Paste>(u => u.Pastes)
                .WithOptional(u => u.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany<ShortenedUrl>(u => u.ShortenedUrls)
                .WithOptional(u => u.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany<Vault>(u => u.Vaults)
                .WithOptional(u => u.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(u => u.OwnedInviteCodes)
                .WithOptional(c => c.Owner)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasOptional(u => u.ClaimedInviteCode)
                .WithOptionalPrincipal(c => c.ClaimedUser)
                .WillCascadeOnDelete(false);

            // Upload Mappings
            modelBuilder.Entity<Upload>()
                .HasOptional(u => u.User);

            // Paste Mappings
            modelBuilder.Entity<Paste>()
                .HasOptional(u => u.User);

            // Shortened URL Mappings
            modelBuilder.Entity<ShortenedUrl>()
                .HasOptional(u => u.User);

            // Vault Mappings
            modelBuilder.Entity<Vault>()
                .HasOptional(u => u.User);
            
            // Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<TrustedDevice>().ToTable("TrustedDevices");
            modelBuilder.Entity<AuthToken>().ToTable("AuthTokens");
            modelBuilder.Entity<InviteCode>().ToTable("InviteCodes");
            modelBuilder.Entity<TransferType>().ToTable("TransferTypes");
            modelBuilder.Entity<RecoveryEmailVerification>().ToTable("RecoveryEmailVerifications");
            modelBuilder.Entity<ResetPasswordVerification>().ToTable("ResetPasswordVerifications");
            // User Settings
            modelBuilder.Entity<UserSettings>().ToTable("Users");
            modelBuilder.Entity<SecuritySettings>().ToTable("Users");
            modelBuilder.Entity<BlogSettings>().ToTable("Users");
            modelBuilder.Entity<UploadSettings>().ToTable("Users");
            // Blogs
            modelBuilder.Entity<Blog>().ToTable("Blogs");
            modelBuilder.Entity<BlogPost>().ToTable("BlogPosts");
            modelBuilder.Entity<PodcastComment>().ToTable("BlogComments");
            // Contact
            modelBuilder.Entity<Contact>().ToTable("Contact");
            // Uploads
            modelBuilder.Entity<Upload>().ToTable("Uploads");
            // Pastes
            modelBuilder.Entity<Paste>().ToTable("Pastes");
            // Shortened Urls
            modelBuilder.Entity<ShortenedUrl>().ToTable("ShortenedUrls");
            // Vaults
            modelBuilder.Entity<Vault>().ToTable("Vaults");
            modelBuilder.Entity<VaultItem>().ToTable("VaultItems");
            // Podcasts
            modelBuilder.Entity<Podcast>().ToTable("Podcasts");
            modelBuilder.Entity<PodcastFile>().ToTable("PodcastFiles");
            modelBuilder.Entity<PodcastComment>().ToTable("PodcastComments");
            // Transparency
            modelBuilder.Entity<Transaction>().ToTable("Transactions");
            modelBuilder.Entity<Takedown>().ToTable("Takedowns");

            // Custom Attributes
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<CaseSensitiveAttribute, bool>(
                                        "CaseSensitive",
                                        (property, attributes) => attributes.Single().IsEnabled));

            base.OnModelCreating(modelBuilder);
        }
    }
}
