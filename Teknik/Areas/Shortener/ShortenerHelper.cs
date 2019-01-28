﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Shortener.Models;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Data;

namespace Teknik.Areas.Shortener
{
    public static class ShortenerHelper
    {
        public static ShortenedUrl ShortenUrl(TeknikEntities db, string url, int length)
        {
            // Generate the shortened url
            string shortUrl = StringHelper.RandomString(length);
            while (db.ShortenedUrls.Where(s => s.ShortUrl == shortUrl).FirstOrDefault() != null)
            {
                shortUrl = StringHelper.RandomString(length);
            }

            ShortenedUrl newUrl = new ShortenedUrl();
            newUrl.OriginalUrl = url;
            newUrl.ShortUrl = shortUrl;
            newUrl.DateAdded = DateTime.Now;

            return newUrl;
        }
    }
}
