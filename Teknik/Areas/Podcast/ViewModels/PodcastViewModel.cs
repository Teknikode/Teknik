using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Podcast.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Podcast.ViewModels
{
    public class PodcastViewModel : ViewModelBase
    {
        public int PodcastId { get; set; }

        public int Episode { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<PodcastFile> Files { get; set; }

        public List<string> Tags { get; set; }

        public DateTime DatePosted { get; set; }
        
        public bool Published { get; set; }

        public DateTime DatePublished { get; set; }

        public DateTime DateEdited { get; set; }

        public PodcastViewModel() { }

        public PodcastViewModel(Models.Podcast podcast)
        {
            PodcastId = podcast.PodcastId;
            Episode = podcast.Episode;
            Title = podcast.Title;
            Description = podcast.Description;
            Files = podcast.Files;
            Tags = podcast.Tags;
            DatePosted = podcast.DatePosted;
            Published = podcast.Published;
            DatePublished = podcast.DatePublished;
            DateEdited = podcast.DateEdited;
        }
    }
}