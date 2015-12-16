using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Blog.ViewModels
{
    public class BlogViewModel
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public List<Post> Posts { get; set; }
    }
}
