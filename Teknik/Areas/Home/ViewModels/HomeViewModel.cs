using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Podcast.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Home.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public List<BlogPost> SitePosts { get; set; }
        public List<Podcast.Models.Podcast> Podcasts { get; set; }
        public List<BlogPost> BlogPosts { get; set; }
    }
}
