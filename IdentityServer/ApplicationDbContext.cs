using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Teknik.IdentityServer.Models;
using Teknik.Utilities.Attributes;

namespace Teknik.IdentityServer
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<AuthToken> AuthTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.AuthTokens).WithOne(t => t.ApplicationUser).HasForeignKey(t => t.ApplicationUserId);

            // Auth Tokens
            modelBuilder.Entity<AuthToken>().ToTable("AuthTokens");

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
        }
    }
}