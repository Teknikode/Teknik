using System;
using System.Collections.Generic;
using Teknik.Areas.Users.Models;

namespace Teknik.Areas.Blog.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<BlogPost> BlogPosts { get; set; }
    }
}