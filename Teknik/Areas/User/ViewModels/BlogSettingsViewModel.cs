using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class BlogSettingsViewModel : SettingsViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public BlogSettingsViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
        }
    }
}
