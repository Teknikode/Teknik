using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Podcast.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string Title { get; set; }
        
        public string Description { get; set; }

        public bool HasPodcasts { get; set; }
    }
}