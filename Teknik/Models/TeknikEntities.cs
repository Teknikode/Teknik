using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Profile.Models;
using Teknik.Areas.Contact.Models;
using Teknik.Migrations;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Paste.Models;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Transparency.Models;

namespace Teknik.Models
{
    public class TeknikEntities : DbContext
    {
        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Role> Roles { get; set; }
        // User Settings
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<BlogSettings> BlogSettings { get; set; }
        public DbSet<UploadSettings> UploadSettings { get; set; }
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasRequired(a => a.UserSettings)
                .WithRequiredPrincipal(a => a.User);
            modelBuilder.Entity<User>()
                .HasRequired(a => a.BlogSettings)
                .WithRequiredPrincipal(a => a.User);
            modelBuilder.Entity<User>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.User);

            modelBuilder.Entity<UserSettings>()
                .HasRequired(a => a.BlogSettings)
                .WithRequiredPrincipal(a => a.UserSettings);
            modelBuilder.Entity<UserSettings>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.UserSettings);
            
            modelBuilder.Entity<BlogSettings>()
                .HasRequired(a => a.UploadSettings)
                .WithRequiredPrincipal(a => a.BlogSettings);

            // Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Role>().ToTable("Roles");
            // User Settings
            modelBuilder.Entity<UserSettings>().ToTable("Users");
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
            // Podcasts
            modelBuilder.Entity<Podcast>().ToTable("Podcasts");
            modelBuilder.Entity<PodcastFile>().ToTable("PodcastFiles");
            modelBuilder.Entity<PodcastComment>().ToTable("PodcastComments");
            // Transparency
            modelBuilder.Entity<Transaction>().ToTable("Transactions");

            base.OnModelCreating(modelBuilder);
        }
    }
}