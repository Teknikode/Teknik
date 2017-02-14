using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Vault.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class NewVaultViewModel : ViewModelBase
    {
        public string title { get; set; }
        public string description { get; set; }
        public List<NewVaultItemViewModel> items { get; set; }

        public NewVaultViewModel()
        {
            title = string.Empty;
            description = string.Empty;
            items = new List<NewVaultItemViewModel>();
        }
    }
}