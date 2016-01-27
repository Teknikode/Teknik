using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Transparency.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Transparency.ViewModels
{
    public class TransparencyViewModel : ViewModelBase
    {
        public int UploadCount { get; set; }

        public int UploadSize { get; set; }

        public int PasteCount { get; set; }

        public int UserCount { get; set; }

        public Dictionary<string, double> TotalBills { get; set; }

        public Dictionary<string, double> TotalOneTimes { get; set; }

        public Dictionary<string, double> TotalDonations { get; set; }

        public Dictionary<string, double> TotalNet { get; set; }

        public List<Bill> Bills { get; set; }

        public List<OneTime> OneTimes { get; set; }

        public List<Donation> Donations { get; set; }

        public List<Takedown> Takedowns { get; set; }
    }
}
