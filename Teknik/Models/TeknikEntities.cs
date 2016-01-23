using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Profile.Models;
using Teknik.Areas.Contact.Models;
using Teknik.Migrations;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Paste.Models;

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
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> BlogComments { get; set; }
        // Contact
        public DbSet<Contact> Contact { get; set; }
        // Uploads
        public DbSet<Upload> Uploads { get; set; }
        // Pastes
        public DbSet<Paste> Pastes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Blog>().ToTable("Blogs");
            modelBuilder.Entity<Post>().ToTable("Posts");
            modelBuilder.Entity<Comment>().ToTable("BlogComments");
            modelBuilder.Entity<Contact>().ToTable("Contact");
            modelBuilder.Entity<Upload>().ToTable("Uploads");
            modelBuilder.Entity<Paste>().ToTable("Pastes");

            base.OnModelCreating(modelBuilder);
        }
    }
}