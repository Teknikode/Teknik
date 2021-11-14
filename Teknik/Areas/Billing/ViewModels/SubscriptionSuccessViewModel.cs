using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.BillingCore.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class SubscriptionSuccessViewModel : ViewModelBase
    {
        public string ProductName { get; set; }

        public decimal? Price { get; set; }

        public string Interval { get; set; }

        public long Storage { get; set; }
    }
}
