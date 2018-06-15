using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Blog.Models
{
    public class BlogPostTag
    {
        public int BlogPostTagId { get; set; }

        public int BlogPostId { get; set; }

        public virtual BlogPost BlogPost { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
