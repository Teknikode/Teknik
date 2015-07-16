using System;
using System.Collections.Generic;

namespace Teknik.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public List<Post> Posts { get; set; }
    }
}