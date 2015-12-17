using System;
using System.Collections.Generic;
using Teknik.Areas.Blog.Models;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Blog.ViewModels
{
    public class PostViewModel
    {
        public int PostId { get; set; }

        public int BlogId { get; set; }

        public DateTime DatePosted { get; set; }

        public DateTime DatePublished { get; set; }

        public bool Published { get; set; }

        public string Title { get; set; }

        public string Article { get; set; }

        public List<string> Tags { get; set; }
    }
}