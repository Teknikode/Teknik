using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.API.V1.Models
{
    public class ShortenAPIv1Model : BaseAPIv1Model
    {
        public string url { get; set; }

        public ShortenAPIv1Model()
        {
            url = string.Empty;
        }
    }
}