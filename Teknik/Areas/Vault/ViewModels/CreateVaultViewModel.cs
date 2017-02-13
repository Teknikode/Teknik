using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Vault.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class CreateVaultViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<VaultItem> Items { get; set; }

        public CreateVaultViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
            Items = new List<VaultItem>();
        }
    }
}