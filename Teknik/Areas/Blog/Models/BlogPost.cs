using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teknik.Areas.Blog.Models
{
    public class BlogPost
    {
        public int BlogPostId { get; set; }

        public int BlogId { get; set; }

        public virtual Blog Blog { get; set; }

        public bool System { get; set; }

        public DateTime DatePosted { get; set; }
        
        public DateTime DatePublished { get; set; }

        public DateTime DateEdited { get; set; }

        public bool Published { get; set; }

        public string Title { get; set; }

        public string Article { get; set; }

        public List<string> Tags { get; set; }

        public virtual ICollection<BlogPostComment> Comments { get; set; }
    }
}