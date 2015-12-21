using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Configuration;

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
    }
}