using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Podcast.Models
{
    public class Podcast
    {
        public int PodcastId { get; set; }

        public int Episode { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<PodcastFile> Files { get; set; }

        public List<string> Tags { get; set; }

        public bool Published { get; set; }

        public DateTime DatePosted { get; set; }

        public DateTime DatePublished { get; set; }

        public DateTime DateEdited { get; set; }

        public List<PodcastComment> Comments { get; set; }
    }
}