using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class AuthTokenViewModel : ViewModelBase
    {
        public int AuthTokenId { get; set; }

        public string Name { get; set; }

        public DateTime? LastDateUsed { get; set; }
    }
}