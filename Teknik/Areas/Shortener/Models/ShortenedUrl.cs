﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Attributes;

namespace Teknik.Areas.Shortener.Models
{
    [Index(nameof(ShortUrl))]
    public class ShortenedUrl
    {
        public int ShortenedUrlId { get; set; }

        public int? UserId { get; set; }

        public virtual User User { get; set; }

        [MaxLength(250)]
        [CaseSensitive]
        public string ShortUrl { get; set; }
        
        public string OriginalUrl { get; set; } 

        public DateTime DateAdded { get; set; }

        public int Views { get; set; }
    }
}