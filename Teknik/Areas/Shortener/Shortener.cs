using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Shortener.Models;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.Shortener
{
    public static class Shortener
    {
        public static ShortenedUrl ShortenUrl(string url, int length)
        {
            TeknikEntities db = new TeknikEntities();
            
            // Generate the shortened url
            string shortUrl = Utility.RandomString(length);
            while (db.ShortenedUrls.Where(s => s.ShortUrl == shortUrl).FirstOrDefault() != null)
            {
                shortUrl = Utility.RandomString(length);
            }

            ShortenedUrl newUrl = new ShortenedUrl();
            newUrl.OriginalUrl = url;
            newUrl.ShortUrl = shortUrl;
            newUrl.DateAdded = DateTime.Now;

            return newUrl;
        }
    }
}
