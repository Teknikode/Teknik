using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teknik.Areas.Blog.Models
{
    public class Post
    {
        public int PostId { get; set; }

        public int BlogId { get; set; }

        public Blog Blog { get; set; }

        public bool System { get; set; }

        public DateTime DatePosted { get; set; }
        
        public DateTime DatePublished { get; set; }
        
        public bool Published { get; set; }

        public string Title { get; set; }

        public string Article { get; set; }

        public List<string> Tags { get; set; }

        public List<Comment> Comments { get; set; }
    }
}