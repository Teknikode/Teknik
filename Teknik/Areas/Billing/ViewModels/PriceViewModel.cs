using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class PriceViewModel : ViewModelBase
    {
        public string ProductName { get; set; }

        public string Name { get; set; }

        public long Storage { get; set; }
    }
}
