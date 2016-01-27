using System;
using System.Collections.Generic;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Blog.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public List<BlogPost> BlogPosts { get; set; }
    }
}