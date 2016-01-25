using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Configuration
{
    public class PodcastConfig
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int PodcastsToLoad { get; set; }
        public int CommentsToLoad { get; set; }
        public string PodcastDirectory { get; set; }

        public PodcastConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Title = string.Empty;
            Description = string.Empty;
            PodcastsToLoad = 10;
            CommentsToLoad = 10;
            PodcastDirectory = string.Empty;
        }
    }
}