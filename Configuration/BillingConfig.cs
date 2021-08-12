using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class BillingConfig
    {
        public string StripePublishApiKey { get; set; }
        public string StripeSecretApiKey { get; set; }

        public BillingConfig()
        {
            StripePublishApiKey = null;
            StripeSecretApiKey = null;
        }
    }
}
