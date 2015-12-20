using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Blog.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }

        public DateTime DatePosted { get; set; }

        public string Article { get; set; }
    }
}