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
        public DbSet<InviteCode> InviteCodes { get; set; }
        public DbSet<UserFeature> UserFeatures { get; set; }
        // User Settings
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<BlogSettings> BlogSettings { get; set; }
        public DbSet<UploadSettings> UploadSettings { get; set; }
        // Features
        public DbSet<Feature> Features { get; set; }
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
            modelBuilder.Entity<User>().OwnsOne(u => u.UserSettings);
            modelBuilder.Entity<User>().OwnsOne(u => u.BlogSettings);
            modelBuilder.Entity<User>().OwnsOne(u => u.UploadSettings);
            modelBuilder.Entity<User>().HasMany(u => u.Features).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.Uploads).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.Pastes).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.ShortenedUrls).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.Vaults).WithOne(u => u.User);
            modelBuilder.Entity<User>().HasMany(u => u.OwnedInviteCodes).WithOne(i => i.Owner);
            modelBuilder.Entity<User>().HasOne(u => u.ClaimedInviteCode).WithOne(i => i.ClaimedUser);
            modelBuilder.Entity<User>().HasOne(u => u.ClaimedInviteCode).WithOne(t => t.ClaimedUser); // Legacy???
            modelBuilder.Entity<User>().HasMany(u => u.OwnedInviteCodes).WithOne(t => t.Owner); // Legacy???

            // Invite Codes
            //modelBuilder.Entity<InviteCode>().HasOne(t => t.ClaimedUser).WithOne(u => u.ClaimedInviteCode).HasPrincipalKey("ClaimedUserId").HasForeignKey("ClaimedUser_UserId"); // Legacy???
            //modelBuilder.Entity<InviteCode>().HasOne(t => t.Owner).WithMany(u => u.OwnedInviteCodes).HasPrincipalKey("ClaimedUserId").HasForeignKey("Owner_UserId"); // Legacy???

            // Features
            modelBuilder.Entity<UserFeature>().HasOne(f => f.Feature);
            modelBuilder.Entity<UserFeature>().HasOne(f => f.User);

            // Blogs
            modelBuilder.Entity<BlogPost>().HasMany(p => p.Comments).WithOne(c => c.BlogPost);

            // Uploads
            modelBuilder.Entity<Upload>().HasOne(u => u.User);

            // Pastes
            modelBuilder.Entity<Paste>().HasOne(u => u.User);

            // Shortened URLs
            modelBuilder.Entity<ShortenedUrl>().HasOne(u => u.User);

            // Vaults
            modelBuilder.Entity<Vault>().HasOne(u => u.User);

            // Takedowns
            modelBuilder.Entity<Takedown>().HasMany(t => t.Attachments).WithOne().HasForeignKey("Takedown_TakedownId"); // Legacy???

            // Transactions
            modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("decimal(19, 5)");

            // Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<InviteCode>().ToTable("InviteCodes");
            modelBuilder.Entity<UserFeature>().ToTable("UserFeatures");
            // User Settings
            modelBuilder.Entity<UserSettings>().ToTable("Users");
            modelBuilder.Entity<BlogSettings>().ToTable("Users");
            modelBuilder.Entity<UploadSettings>().ToTable("Users");
            // Features
            modelBuilder.Entity<Feature>().ToTable("Features");
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
