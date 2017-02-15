using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Vault.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class ModifyVaultViewModel : ViewModelBase
    {
        public bool isEdit { get; set; }
        public int vaultId { get; set; }
        public string CurrentSub { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<ModifyVaultItemViewModel> items { get; set; }

        public ModifyVaultViewModel()
        {
            isEdit = false;
            vaultId = -1;
            CurrentSub = "vault";
            title = string.Empty;
            description = string.Empty;
            items = new List<ModifyVaultItemViewModel>();
        }
    }
}