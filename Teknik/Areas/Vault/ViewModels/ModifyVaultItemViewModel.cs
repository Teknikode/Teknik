﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Vault.ViewModels
{
    public class ModifyVaultItemViewModel : ViewModelBase
    {
        public bool isTemplate { get; set; }
        public int index { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string url { get; set; }

        public ModifyVaultItemViewModel()
        {
            isTemplate = true;
            index = 0;
            title = string.Empty;
            description = string.Empty;
            type = "Upload";
            url = string.Empty;
        }
    }
}