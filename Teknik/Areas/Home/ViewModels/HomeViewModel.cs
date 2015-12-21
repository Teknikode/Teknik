using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Blog.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Home.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public List<Post> SitePosts { get; set; }
        public List<Post> Podcasts { get; set; }
        public List<Post> BlogPosts { get; set; }
    }
}
