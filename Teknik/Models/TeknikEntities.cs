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

namespace Teknik.Models
{
    public class TeknikEntities : DbContext
    {
        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Role> Roles { get; set; }
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Blog>().ToTable("Blogs");
            modelBuilder.Entity<BlogPost>().ToTable("BlogPosts");
            modelBuilder.Entity<PodcastComment>().ToTable("BlogComments");
            modelBuilder.Entity<Contact>().ToTable("Contact");
            modelBuilder.Entity<Upload>().ToTable("Uploads");
            modelBuilder.Entity<Paste>().ToTable("Pastes");
            modelBuilder.Entity<Podcast>().ToTable("Podcasts");
            modelBuilder.Entity<PodcastFile>().ToTable("PodcastFiles");
            modelBuilder.Entity<PodcastComment>().ToTable("PodcastComments");

            base.OnModelCreating(modelBuilder);
        }
    }
}