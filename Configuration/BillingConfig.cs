using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class BillingConfig
    {
        public bool Enabled { get; set; }
        public BillingType Type { get; set; }
        public string StripePublishApiKey { get; set; }
        public string StripeSecretApiKey { get; set; }
        public string StripeCheckoutWebhookSecret { get; set; }
        public string StripeSubscriptionWebhookSecret { get; set; }
        public string StripeCustomerWebhookSecret { get; set; }

        public string UploadProductId { get; set; }
        public string EmailProductId { get; set; }

        public BillingConfig()
        {
            Enabled = false;
            Type = BillingType.Stripe;
            StripePublishApiKey = null;
            StripeSecretApiKey = null;
            StripeCheckoutWebhookSecret = null;
            StripeSubscriptionWebhookSecret = null;
            StripeCustomerWebhookSecret = null;
        }
    }
}
