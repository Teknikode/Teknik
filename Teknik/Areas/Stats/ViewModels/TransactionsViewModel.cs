using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Stats.ViewModels
{
    public class TransactionsViewModel : ViewModelBase
    {
        public decimal CurrentMonthBills { get; set; }

        public decimal CurrentMonthIncome { get; set; }

        public decimal TotalBills { get; set; }

        public decimal TotalOneTimes { get; set; }

        public decimal TotalDonations { get; set; }

        public decimal TotalNet { get; set; }

        public List<BillViewModel> Bills { get; set; }

        public List<OneTimeViewModel> OneTimes { get; set; }

        public List<DonationViewModel> Donations { get; set; }

        public TransactionsViewModel()
        {
            TotalBills = 0;
            TotalOneTimes = 0;
            TotalDonations = 0;
            TotalNet = 0;
            Bills = new List<BillViewModel>();
            OneTimes = new List<OneTimeViewModel>();
            Donations = new List<DonationViewModel>();
        }
    }
}
