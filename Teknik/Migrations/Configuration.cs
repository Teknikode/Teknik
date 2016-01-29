namespace Teknik.Migrations
{
    using Areas.Paste;
    using Helpers;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
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

            // Create server blog
            Areas.Blog.Models.Blog serverBlog = new Areas.Blog.Models.Blog();
            context.Blogs.Add(serverBlog);
            context.SaveChanges();

            // Create roles and groups
            Areas.Profile.Models.Role adminRole = new Areas.Profile.Models.Role();
            adminRole.Name = "Admin";
            adminRole.Description = "Allows complete access to user specific actions";
            context.Roles.Add(adminRole);

            Areas.Profile.Models.Role podcastRole = new Areas.Profile.Models.Role();
            podcastRole.Name = "Podcast";
            podcastRole.Description = "Allows create/edit/delete access to podcasts";
            context.Roles.Add(podcastRole);

            Areas.Profile.Models.Group adminGroup = new Areas.Profile.Models.Group();
            adminGroup.Name = "Administrators";
            adminGroup.Description = "System Administrators with full access";
            adminGroup.Roles.Add(adminRole);
            adminGroup.Roles.Add(podcastRole);

            Areas.Profile.Models.Group podcastGroup = new Areas.Profile.Models.Group();
            podcastGroup.Name = "Podcast";
            podcastGroup.Description = "Podcast team members";
            podcastGroup.Roles.Add(podcastRole);

            context.SaveChanges();


            if (config.DatabaseConfig.Migrate && !config.DevEnvironment)
            {
                // Convert legacy MySQL DB to new MS SQL DB
                MysqlDatabase db = new MysqlDatabase(config.DatabaseConfig);

                // Transfer transactions
                var transRet = db.Query("SELECT * FROM transactions");
                foreach (var tran in transRet)
                {
                    switch (tran["trans_type"].ToString())
                    {
                        case "One-Time":
                            Areas.Transparency.Models.OneTime tr = new Areas.Transparency.Models.OneTime();
                            tr.DateSent = DateTime.Parse(tran["date_posted"].ToString());
                            tr.Amount = Int32.Parse(tran["amount"].ToString());
                            tr.Currency = tran["currency"].ToString();
                            tr.Recipient = tran["recipient"].ToString();
                            tr.Reason = tran["reason"].ToString();
                            context.Transactions.Add(tr);
                            break;
                        case "Bill":
                            Areas.Transparency.Models.Bill bill = new Areas.Transparency.Models.Bill();
                            bill.DateSent = DateTime.Parse(tran["date_posted"].ToString());
                            bill.Amount = Int32.Parse(tran["amount"].ToString());
                            bill.Currency = tran["currency"].ToString();
                            bill.Recipient = tran["recipient"].ToString();
                            bill.Reason = tran["reason"].ToString();
                            context.Transactions.Add(bill);
                            break;
                        case "Donation":
                            Areas.Transparency.Models.Donation don = new Areas.Transparency.Models.Donation();
                            don.DateSent = DateTime.Parse(tran["date_posted"].ToString());
                            don.Amount = Int32.Parse(tran["amount"].ToString());
                            don.Currency = tran["currency"].ToString();
                            don.Sender = tran["sender"].ToString();
                            don.Reason = tran["reason"].ToString();
                            context.Transactions.Add(don);
                            break;
                    }
                }
                context.SaveChanges();

                // Transfer Users and Blogs/Posts
                Dictionary<int, int> userMapping = new Dictionary<int, int>();
                Dictionary<int, int> postMapping = new Dictionary<int, int>();
                var userRet = db.Query("SELECT * FROM users");
                foreach (var user in userRet)
                {
                    // Create User
                    Areas.Profile.Models.User newUser = new Areas.Profile.Models.User();
                    newUser.UserSettings = new Areas.Profile.Models.UserSettings();
                    newUser.BlogSettings = new Areas.Profile.Models.BlogSettings();
                    newUser.UploadSettings = new Areas.Profile.Models.UploadSettings();
                    newUser.TransferAccount = true;
                    newUser.Username = user["username"].ToString();
                    newUser.HashedPassword = user["password"].ToString();
                    newUser.JoinDate = DateTime.Parse(user["join_date"].ToString());
                    newUser.LastSeen = DateTime.Parse(user["last_seen"].ToString());
                    newUser.UserSettings.About = user["about"].ToString();
                    newUser.UserSettings.Website = user["website"].ToString();
                    newUser.UserSettings.Quote = user["quote"].ToString();
                    newUser.BlogSettings.Title = user["blog_title"].ToString();
                    newUser.BlogSettings.Description = user["blog_desc"].ToString();
                    context.Users.Add(newUser);
                    context.SaveChanges();
                    int userId = newUser.UserId;

                    userMapping.Add(Int32.Parse(user["id"].ToString()), userId);

                    // Create Blog for user
                    Areas.Blog.Models.Blog newBlog = new Areas.Blog.Models.Blog();
                    newBlog.UserId = userId;
                    context.SaveChanges();
                    int blogId = newBlog.BlogId;

                    // Transfer Blog Posts
                    var postRet = db.Query("SELECT * FROM blog WHERE author_id={0}", new object[] { userId });
                    if (postRet != null)
                    {
                        foreach (var post in postRet)
                        {
                            // Create new Blog Post
                            Areas.Blog.Models.BlogPost newPost = new Areas.Blog.Models.BlogPost();
                            if (post["user_id"].ToString() == "0")
                            {
                                newPost.BlogId = 0;
                                newPost.System = true;
                            }
                            else
                            {
                                newPost.BlogId = blogId;
                            }
                            newPost.DatePosted = DateTime.Parse(post["date_posted"].ToString());
                            newPost.DatePublished = DateTime.Parse(post["date_published"].ToString());
                            newPost.DateEdited = DateTime.Parse(post["date_published"].ToString());
                            newPost.Published = (post["published"].ToString() == "1");
                            newPost.Title = post["title"].ToString();
                            newPost.Article = post["post"].ToString();
                            context.BlogPosts.Add(newPost);
                            context.SaveChanges();
                            postMapping.Add(Int32.Parse(post["id"].ToString()), newPost.BlogPostId);
                        }
                    }
                }

                // Transfer Blog Comments
                var commentRet = db.Query("SELECT * FROM comments WHERE service = 'blog'");
                foreach (var comment in commentRet)
                {
                    int postId = Int32.Parse(comment["reply_id"].ToString());
                    int userId = Int32.Parse(comment["user_id"].ToString());
                    if (postMapping.ContainsKey(postId) && userMapping.ContainsKey(userId))
                    {
                        Areas.Blog.Models.BlogPostComment newComment = new Areas.Blog.Models.BlogPostComment();
                        newComment.BlogPostId = postMapping[postId];
                        newComment.UserId = userMapping[userId];
                        newComment.Article = comment["post"].ToString();
                        newComment.DatePosted = DateTime.Parse(comment["date_posted"].ToString());
                        newComment.DateEdited = DateTime.Parse(comment["date_posted"].ToString());
                        context.BlogComments.Add(newComment);
                        context.SaveChanges();
                    }
                }

                // Transfer Pastes
                var pasteRet = db.Query("SELECT * FROM paste");
                foreach (var paste in pasteRet)
                {
                    // If it's a password protected paste, we just skip it
                    if (paste["password"] == null)
                    {
                        string content = paste["code"].ToString();
                        string title = paste["title"].ToString();
                        DateTime posted = DateTime.Parse(paste["posted"].ToString());
                        int userId = Int32.Parse(paste["user_id"].ToString());
                        Areas.Paste.Models.Paste newPaste = PasteHelper.CreatePaste(content, title);
                        newPaste.DatePosted = posted;
                        newPaste.Url = paste["pid"].ToString();
                        if (userMapping.ContainsKey(userId) && userId != 0)
                        {
                            newPaste.UserId = userMapping[userId];
                        }
                        context.Pastes.Add(newPaste);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
