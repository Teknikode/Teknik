﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Paste.ViewModels
{
    public class PasswordViewModel : ViewModelBase
    {
        public string ActionUrl { get; set; }

        public string Url { get; set; }

        public string Type { get; set; }
    }
}
