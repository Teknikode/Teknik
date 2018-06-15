using System;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Users.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Blog.ViewModels
{
    public class CommentViewModel : ViewModelBase
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public BlogPost Post { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime DateEdited { get; set; }
        public string Article { get; set; }

        public CommentViewModel(BlogPostComment comment)
        {
            CommentId = comment.BlogPostCommentId;
            PostId = comment.BlogPostId;
            Post = comment.BlogPost;
            UserId = comment.UserId;
            User = comment.User;
            DatePosted = comment.DatePosted;
            DateEdited = comment.DateEdited;
            Article = comment.Article;
        }
    }
}