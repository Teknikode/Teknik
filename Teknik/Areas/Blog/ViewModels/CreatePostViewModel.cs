﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.ViewModels;

namespace Teknik.Areas.Blog.ViewModels
{
    public class CreatePostViewModel : ViewModelBase
    {
        public int BlogId { get; set; }

        public string Title { get; set; }

        [AllowHtml]
        public string Article { get; set; }
    }
}