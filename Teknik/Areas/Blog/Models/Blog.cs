using System;
using System.Collections.Generic;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Blog.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public User User { get; set; }

        public List<Post> Posts { get; set; }
    }
}