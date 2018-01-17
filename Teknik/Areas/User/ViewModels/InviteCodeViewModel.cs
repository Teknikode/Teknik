using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class InviteCodeViewModel : ViewModelBase
    {
        public int InviteCodeId { get; set; }

        public bool Active { get; set; }
        
        public string Code { get; set; }

        public virtual Models.User Owner { get; set; }

        public virtual Models.User ClaimedUser { get; set; }
    }
}
