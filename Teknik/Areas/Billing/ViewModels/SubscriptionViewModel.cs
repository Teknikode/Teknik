using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class SubscriptionViewModel : ViewModelBase
    {
        public bool Recommended { get; set; }
        public bool CurrentSubMonthly { get; set; }
        public bool CurrentSubYearly { get; set; }
        public string SubscriptionId { get; set; }
        public decimal? BasePriceMonthly { get; set; }
        public decimal? BasePriceYearly { get; set; }
        public long? BaseStorage { get; set; }
        public bool OverageAllowed { get; set; }
        public decimal? OveragePriceMonthly { get; set; }
        public decimal? OveragePriceYearly { get; set; }
        public string OverageUnit { get; set; }
        public long? MaxStorage { get; set; }
        public string SubscribeUrlYearly { get; set; }
        public string SubscribeUrlMonthly { get; set; }
        public string SubscribeText { get; set; }

        public string PanelOffset { get; set; }

        public SubscriptionViewModel()
        {
            Recommended = false;
            CurrentSubMonthly = false;
            CurrentSubYearly = false;
            OverageAllowed = false;
        }
    }
}
