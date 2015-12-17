using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Blog.Models;

namespace Teknik.Areas.Home.ViewModels
{
    public class HomeViewModel
    {
        public List<Post> SitePosts { get; set; }
        public List<Post> Podcasts { get; set; }
        public List<Post> BlogPosts { get; set; }
    }
}
