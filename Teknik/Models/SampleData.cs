using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Teknik.Models
{
    public class SampleData : DropCreateDatabaseIfModelChanges<TeknikEntities>
    {
        protected override void Seed(TeknikEntities context)
        {
            var user = new User
            {
                Username = "Uncled1023",
                JoinDate = DateTime.Now,
                LastSeen = DateTime.Now
            };
            context.Users.Add(user);

            var posts = new List<Post>
            {
                new Post { Article = "Test Post", DatePosted = DateTime.Now, DatePublished = DateTime.Now, Title = "Test Post", Published = false}
            };
            posts.ForEach(post => context.Posts.Add(post));

            new List<Blog>
            {
                new Blog {Posts = posts, User = user}
            }.ForEach(blog => context.Blogs.Add(blog));
        }
    }
}