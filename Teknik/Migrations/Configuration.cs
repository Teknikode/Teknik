namespace Teknik.Migrations
{
    using Areas.Paste;
    using Areas.Upload;
    using Areas.Users.Utility;
    using Helpers;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Teknik.Configuration;

    internal sealed class Configuration : DbMigrationsConfiguration<Models.TeknikEntities>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(Models.TeknikEntities context)
        {
            Config config = Config.Load();
            // Pre-populate with the default stuff
            if (!UserHelper.UserExists(context, Constants.SERVERUSER))
            {
                // Create system blog
                Areas.Users.Models.User systemUser = new Areas.Users.Models.User();
                systemUser.Username = Constants.SERVERUSER;
                systemUser.JoinDate = DateTime.Now;
                systemUser.LastSeen = DateTime.Now;
                systemUser.UserSettings = new Areas.Users.Models.UserSettings();
                systemUser.SecuritySettings = new Areas.Users.Models.SecuritySettings();
                systemUser.BlogSettings = new Areas.Users.Models.BlogSettings();
                systemUser.UploadSettings = new Areas.Users.Models.UploadSettings();
                context.Users.Add(systemUser);
                context.SaveChanges();

                Areas.Blog.Models.Blog systemBlog = new Areas.Blog.Models.Blog();
                systemBlog.UserId = systemUser.UserId;
                systemBlog.BlogId = config.BlogConfig.ServerBlogId;
                context.Blogs.Add(systemBlog);
                context.SaveChanges();
            }

            Areas.Users.Models.Role adminRole = context.Roles.Where(r => r.Name == "Admin").FirstOrDefault();
            if (adminRole  == null)
            {
                // Create roles and groups
                adminRole = new Areas.Users.Models.Role();
                adminRole.Name = "Admin";
                adminRole.Description = "Allows complete access to user specific actions";
                context.Roles.Add(adminRole);
            }

            Areas.Users.Models.Role podcastRole = context.Roles.Where(r => r.Name == "Podcast").FirstOrDefault();
            if (podcastRole == null)
            {
                podcastRole = new Areas.Users.Models.Role();
                podcastRole.Name = "Podcast";
                podcastRole.Description = "Allows create/edit/delete access to podcasts";
                context.Roles.Add(podcastRole);
            }

            if (context.Groups.Where(g => g.Name == "Administrators").FirstOrDefault() == null)
            {
                Areas.Users.Models.Group adminGroup = new Areas.Users.Models.Group();
                adminGroup.Name = "Administrators";
                adminGroup.Description = "System Administrators with full access";
                adminGroup.Roles = new List<Areas.Users.Models.Role>();
                adminGroup.Roles.Add(adminRole);
                adminGroup.Roles.Add(podcastRole);
                context.Groups.AddOrUpdate(adminGroup);
            }

            if (context.Groups.Where(g => g.Name == "Podcast").FirstOrDefault() == null)
            {
                Areas.Users.Models.Group podcastGroup = new Areas.Users.Models.Group();
                podcastGroup.Name = "Podcast";
                podcastGroup.Description = "Podcast team members";
                podcastGroup.Roles = new List<Areas.Users.Models.Role>();
                podcastGroup.Roles.Add(podcastRole);
                context.Groups.AddOrUpdate(podcastGroup);
            }

            if (context.Groups.Where(g => g.Name == "Member").FirstOrDefault() == null)
            {
                Areas.Users.Models.Group memberGroup = new Areas.Users.Models.Group();
                memberGroup.Name = "Member";
                memberGroup.Description = "The default member group with basic permissions";
                context.Groups.AddOrUpdate(memberGroup);
            }

            context.SaveChanges();
        }
    }
}
