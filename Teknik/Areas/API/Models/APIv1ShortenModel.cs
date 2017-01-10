using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.API.Models
{
    public class APIv1ShortenModel : APIv1BaseModel
    {
        public string url { get; set; }

        public APIv1ShortenModel()
        {
            url = string.Empty;
        }
    }
}