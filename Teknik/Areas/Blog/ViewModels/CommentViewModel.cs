using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Blog.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public DateTime DatePosted { get; set; }
        public string Article { get; set; }

        public CommentViewModel(Comment comment)
        {
            CommentId = comment.CommentId;
            PostId = comment.PostId;
            Post = comment.Post;
            UserId = comment.UserId;
            User = comment.User;
            DatePosted = comment.DatePosted;
            Article = comment.Article;
        }
    }
}