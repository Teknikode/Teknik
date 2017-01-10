using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teknik.Areas.API.Models
{
    public class APIv1PasteModel : APIv1BaseModel
    {
        [AllowHtml]
        public string code { get; set; }

        public string title { get; set; }

        public string syntax { get; set; }

        public string expireUnit { get; set; }

        public int expireLength { get; set; }

        public string password { get; set; }

        public bool hide { get; set; }

        public APIv1PasteModel()
        {
            code = null;
            title = string.Empty;
            syntax = "text";
            expireUnit = "never";
            expireLength = 1;
            password = string.Empty;
            hide = false;
        }
    }
}