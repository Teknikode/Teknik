using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;

namespace Teknik.Areas.Shortener.Models
{
    public class ShortenedUrl
    {
        public int ShortenedUrlId { get; set; }

        public int? UserId { get; set; }

        public virtual User User { get; set; }
        
        public string ShortUrl { get; set; }
        
        public string OriginalUrl { get; set; } 

        public DateTime DateAdded { get; set; }

        public int Views { get; set; }
    }
}