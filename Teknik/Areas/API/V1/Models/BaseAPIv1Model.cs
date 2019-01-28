using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.API.V1.Models
{
    public class BaseAPIv1Model
    {
        public bool doNotTrack { get; set; }

        public BaseAPIv1Model()
        {
            doNotTrack = false;
        }
    }
}