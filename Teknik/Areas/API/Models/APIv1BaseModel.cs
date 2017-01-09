using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.API.Models
{
    public class APIv1BaseModel
    {
        public bool doNotTrack { get; set; }

        public APIv1BaseModel()
        {
            doNotTrack = false;
        }
    }
}