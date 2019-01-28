using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Podcast.Models
{
    public class PodcastTag
    {
        public int PodcastTagId { get; set; }

        public int PodcastId { get; set; }

        public virtual Podcast Podcast { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
