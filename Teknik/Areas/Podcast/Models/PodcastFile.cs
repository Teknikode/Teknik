using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Podcast.Models
{
    public class PodcastFile
    {
        public int PodcastFileId { get; set; }

        public int PodcastId { get; set; }

        public virtual Podcast Podcast { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public string ContentType { get; set; }

        public int ContentLength { get; set; }

        public int Size { get; set; }
    }
}