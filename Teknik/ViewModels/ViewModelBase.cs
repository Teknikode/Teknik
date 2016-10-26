using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Configuration;
using Teknik.Helpers;

namespace Teknik.ViewModels
{
    public abstract class ViewModelBase
    {
        private Config _config;

        public Config Config
        {
            get
            {
                if (_config == null)
                {
                    _config = Config.Load();
                }
                return _config;
            }
        }

        public bool Error { get; set; }

        public string ErrorMessage { get; set; }

        public ViewModelBase()
        {
            Error = false;
            ErrorMessage = string.Empty;
        }
    }
}