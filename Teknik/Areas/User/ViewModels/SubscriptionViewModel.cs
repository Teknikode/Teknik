using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class SubscriptionViewModel : SettingsViewModel
    {
        public string SubscriptionId { get; set; }

        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal? Price { get; set; }

        public string Interval { get; set; }

        public long Storage { get; set; }
    }
}
