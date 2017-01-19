using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.API.Models
{
    public class APIv1BaseModel
    {
        public bool doNotTrack { get; set; }

        public string username { get; set; }

        public string authToken { get; set; }

        public APIv1BaseModel()
        {
            doNotTrack = false;
            username = string.Empty;
            authToken = string.Empty;
        }
    }
}