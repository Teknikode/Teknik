using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Teknik.Models
{
    public class TeknikEntities : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Blog>().ToTable("Blogs");
            modelBuilder.Entity<Post>().ToTable("Posts");

            base.OnModelCreating(modelBuilder);
        }
    }
}