using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;

namespace Teknik.Areas.Blog.Models
{
    public class BlogPostComment
    {
        public int BlogPostCommentId { get; set; }

        public int BlogPostId { get; set; }

        public BlogPost BlogPost { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }

        public DateTime DatePosted { get; set; }
        public DateTime DateEdited { get; set; }

        public string Article { get; set; }
    }
}