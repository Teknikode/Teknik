using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;

namespace Teknik.Areas.Podcast.Models
{
    public class PodcastComment
    {
        public int PodcastCommentId { get; set; }

        public int PodcastId { get; set; }

        public virtual Podcast Podcast { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime DatePosted { get; set; }

        public DateTime DateEdited { get; set; }

        public string Article { get; set; }
    }
}