using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.BillingCore
{
    public static class BillingFactory
    {
        public static BillingService GetBillingService(BillingConfig config)
        {
            switch (config.Type)
            {
                case BillingType.Stripe:
                    return new StripeService(config);
                default:
                    return null;
            }
        }
    }
}
