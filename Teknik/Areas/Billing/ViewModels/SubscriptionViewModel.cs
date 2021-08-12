using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class SubscriptionViewModel : ViewModelBase
    {
        public bool Primary { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public double? BasePrice { get; set; }
        public string BaseUnit { get; set; }
        public long? BaseStorage { get; set; }
        public bool OverageAllowed { get; set; }
        public double? OveragePrice { get; set; }
        public string OverageUnit { get; set; }
        public long? MaxStorage { get; set; }
        public string SubscribeUrl { get; set; }
        public string SubscribeText { get; set; }

        public SubscriptionViewModel()
        {
            Primary = false;
            OverageAllowed = false;
        }
    }
}
