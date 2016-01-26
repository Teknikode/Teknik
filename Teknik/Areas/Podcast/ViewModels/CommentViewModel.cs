using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Profile.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Podcast.ViewModels
{
    public class CommentViewModel : ViewModelBase
    {
        public int CommentId { get; set; }
        public int PodcastId { get; set; }
        public Models.Podcast Podcast { get; set; }
        public User User { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime DateEdited { get; set; }
        public string Article { get; set; }

        public CommentViewModel(Models.PodcastComment comment)
        {
            CommentId = comment.PodcastCommentId;
            PodcastId = comment.PodcastId;
            Podcast = comment.Podcast;
            User = comment.User;
            DatePosted = comment.DatePosted;
            DateEdited = comment.DateEdited;
            Article = comment.Article;
        }
    }
}