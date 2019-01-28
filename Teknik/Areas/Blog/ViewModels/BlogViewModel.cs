using System.Collections.Generic;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Users.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Blog.ViewModels
{
    public class BlogViewModel : ViewModelBase
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public User User { get; set; }

        public bool HasPosts
        {
            get
            {
                return Posts != null && Posts.Count > 0;
            }
        }

        public List<PostViewModel> Posts { get; set; }

        public BlogViewModel()
        {

        }

        public BlogViewModel(Models.Blog blog)
        {
            BlogId = blog.BlogId;
            UserId = blog.UserId;
            Title = blog.User.BlogSettings.Title;
            Description = blog.User.BlogSettings.Description;
            User = blog.User;
            Posts = new List<PostViewModel>();
        }
    }
}
