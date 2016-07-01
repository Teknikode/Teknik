using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class TwoFactorViewModel : ViewModelBase
    {
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public bool AllowTrustedDevice { get; set; }
    }
}