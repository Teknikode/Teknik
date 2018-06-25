using Teknik.Areas.Blog.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Contact.Models;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Paste.Models;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Stats.Models;
using Teknik.Areas.Shortener.Models;
using Teknik.Attributes;
using System.Linq;
using Teknik.Areas.Vault.Models;
using Microsoft.EntityFrameworkCore;
using Teknik.Models;
using System.Configuration;

namespace Teknik.Data
{
    public class TeknikEntities : DbContext
    {
        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<LoginInfo> UserLogins { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
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
        public DbSet<BlogPostTag> BlogPostTagss { get; set; }
        public DbSet<BlogPostComment> BlogPostComments { get; set; }
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

        public TeknikEntities(DbContextOptions<TeknikEntities> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["TeknikEntities"].ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserSettings).WithOne(u => u.User).HasForeignKey<UserSettings>(u => u.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.SecuritySettings).WithOne(u => u.User).HasForeignKey<SecuritySettings>(u => u.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.BlogSettings).WithOne(u => u.User).HasForeignKey<BlogSettings>(u => u.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.UploadSettings).WithOne(u => u.User).HasForeignKey<UploadSettings>(u => u.UserId);
            modelBuilder.Entity<User>().HasMany(u => u.Uploads).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.Pastes).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.ShortenedUrls).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.Vaults).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.OwnedInviteCodes).WithOne(i => i.Owner);
            modelBuilder.Entity<User>().HasMany(u => u.Transfers).WithOne(i => i.User);
            modelBuilder.Entity<User>().HasOne(u => u.ClaimedInviteCode).WithOne(i => i.ClaimedUser);
            modelBuilder.Entity<User>().HasMany(u => u.Logins).WithOne(r => r.User);
            modelBuilder.Entity<User>().HasMany(u => u.UserRoles).WithOne(r => r.User);
            modelBuilder.Entity<User>().HasOne(u => u.ClaimedInviteCode).WithOne(t => t.ClaimedUser); // Legacy???
            modelBuilder.Entity<User>().HasMany(u => u.OwnedInviteCodes).WithOne(t => t.Owner); // Legacy???

            // User Settings
            modelBuilder.Entity<UserSettings>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<UserSettings>()
                .HasOne(u => u.User).WithOne(u => u.UserSettings).HasForeignKey<User>(u => u.UserId);
            modelBuilder.Entity<UserSettings>()
                .HasOne(u => u.SecuritySettings).WithOne(u => u.UserSettings).HasForeignKey<SecuritySettings>(u => u.UserId);
            modelBuilder.Entity<UserSettings>()
                .HasOne(u => u.BlogSettings).WithOne(u => u.UserSettings).HasForeignKey<BlogSettings>(u => u.UserId);
            modelBuilder.Entity<UserSettings>()
                .HasOne(u => u.UploadSettings).WithOne(u => u.UserSettings).HasForeignKey<UploadSettings>(u => u.UserId);

            // Security Settings
            modelBuilder.Entity<SecuritySettings>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<SecuritySettings>()
                .HasOne(u => u.User).WithOne(u => u.SecuritySettings).HasForeignKey<User>(u => u.UserId);
            modelBuilder.Entity<SecuritySettings>()
                .HasOne(u => u.UserSettings).WithOne(u => u.SecuritySettings).HasForeignKey<UserSettings>(u => u.UserId);
            modelBuilder.Entity<SecuritySettings>()
                .HasOne(u => u.BlogSettings).WithOne(u => u.SecuritySettings).HasForeignKey<BlogSettings>(u => u.UserId);
            modelBuilder.Entity<SecuritySettings>()
                .HasOne(u => u.UploadSettings).WithOne(u => u.SecuritySettings).HasForeignKey<UploadSettings>(u => u.UserId);

            // Blog Settings
            modelBuilder.Entity<BlogSettings>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<BlogSettings>()
                .HasOne(u => u.User).WithOne(u => u.BlogSettings).HasForeignKey<User>(u => u.UserId);
            modelBuilder.Entity<BlogSettings>()
                .HasOne(u => u.UserSettings).WithOne(u => u.BlogSettings).HasForeignKey<UserSettings>(u => u.UserId);
            modelBuilder.Entity<BlogSettings>()
                .HasOne(u => u.SecuritySettings).WithOne(u => u.BlogSettings).HasForeignKey<SecuritySettings>(u => u.UserId);
            modelBuilder.Entity<BlogSettings>()
                .HasOne(u => u.UploadSettings).WithOne(u => u.BlogSettings).HasForeignKey<UploadSettings>(u => u.UserId);

            // Upload Settings
            modelBuilder.Entity<UploadSettings>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<UploadSettings>()
                .HasOne(u => u.User).WithOne(u => u.UploadSettings).HasForeignKey<User>(u => u.UserId);
            modelBuilder.Entity<UploadSettings>()
                .HasOne(u => u.UserSettings).WithOne(u => u.UploadSettings).HasForeignKey<UserSettings>(u => u.UserId);
            modelBuilder.Entity<UploadSettings>()
                .HasOne(u => u.SecuritySettings).WithOne(u => u.UploadSettings).HasForeignKey<SecuritySettings>(u => u.UserId);
            modelBuilder.Entity<UploadSettings>()
                .HasOne(u => u.BlogSettings).WithOne(u => u.UploadSettings).HasForeignKey<BlogSettings>(u => u.UserId);

            // UserRoles
            modelBuilder.Entity<UserRole>().HasOne(r => r.User).WithMany(u => u.UserRoles);
            modelBuilder.Entity<UserRole>().HasOne(r => r.Role).WithMany(r => r.UserRoles);

            // Roles
            modelBuilder.Entity<Role>().HasMany(r => r.UserRoles).WithOne(r => r.Role);

            // Invite Codes
            //modelBuilder.Entity<InviteCode>().HasOne(t => t.ClaimedUser).WithOne(u => u.ClaimedInviteCode).HasPrincipalKey("ClaimedUserId").HasForeignKey("ClaimedUser_UserId"); // Legacy???
            //modelBuilder.Entity<InviteCode>().HasOne(t => t.Owner).WithMany(u => u.OwnedInviteCodes).HasPrincipalKey("ClaimedUserId").HasForeignKey("Owner_UserId"); // Legacy???

            // Blogs
            modelBuilder.Entity<BlogPost>().HasMany(p => p.Comments).WithOne(c => c.BlogPost);

            // Uploads
            modelBuilder.Entity<Upload>().HasOne(u => u.User);

            // Pastes
            modelBuilder.Entity<Paste>().HasOne(u => u.User);
            modelBuilder.Entity<Paste>().HasMany(p => p.Transfers).WithOne(t => t.Paste);

            // Shortened URLs
            modelBuilder.Entity<ShortenedUrl>().HasOne(u => u.User);

            // Vaults
            modelBuilder.Entity<Vault>().HasOne(u => u.User);

            // Takedowns
            modelBuilder.Entity<Takedown>().HasMany(t => t.Attachments).WithOne().HasForeignKey("Takedown_TakedownId"); // Legacy???

            // Transfer Types
            modelBuilder.Entity<TransferType>().HasOne(t => t.User).WithMany(u => u.Transfers);
            modelBuilder.Entity<TransferType>().HasOne(t => t.Paste).WithMany(p => p.Transfers);

            // Transactions
            modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("decimal(19, 5)");

            // Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<LoginInfo>().ToTable("UserLogins");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<TrustedDevice>().ToTable("TrustedDevices");
            modelBuilder.Entity<AuthToken>().ToTable("AuthTokens");
            modelBuilder.Entity<InviteCode>().ToTable("InviteCodes");
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
            modelBuilder.Entity<BlogPostComment>().ToTable("BlogPostComments");
            modelBuilder.Entity<BlogPostTag>().ToTable("BlogPostTags");
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
            modelBuilder.Entity<PodcastTag>().ToTable("PodcastTags");
            // Transparency
            modelBuilder.Entity<Transaction>().ToTable("Transactions");
            modelBuilder.Entity<Takedown>().ToTable("Takedowns");
            // Transfer Types
            modelBuilder.Entity<TransferType>().ToTable("TransferTypes");

            // Custom Attributes
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var attributes = property?.PropertyInfo?.GetCustomAttributes(typeof(CaseSensitiveAttribute), false);
                    if (attributes != null && attributes.Any())
                    {
                        property.SetAnnotation("CaseSensitive", true);
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
