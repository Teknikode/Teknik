using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Stats.Models;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Stats.ViewModels
{
    public class StatsViewModel : ViewModelBase
    {
        public int UploadCount { get; set; }

        public long UploadSize { get; set; }

        public int PasteCount { get; set; }

        public int UserCount { get; set; }

        public int ShortenedUrlCount { get; set; }

        public int VaultCount { get; set; }

        public TransactionsViewModel Transactions { get; set; }

        public List<TakedownViewModel> Takedowns { get; set; }

        public StatsViewModel()
        {
            UploadCount = 0;
            UploadSize = 0;
            PasteCount = 0;
            UserCount = 0;
            ShortenedUrlCount = 0;
            VaultCount = 0;
            Transactions = new TransactionsViewModel();
            Takedowns = new List<TakedownViewModel>();
        }
    }
}
