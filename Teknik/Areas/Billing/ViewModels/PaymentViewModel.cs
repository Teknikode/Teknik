using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class PaymentViewModel : ViewModelBase
    {
        public string StripePublishKey { get; set; }
        public string PaymentName { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CCV { get; set; }
        public string ZipCode { get; set; }
    }
}
