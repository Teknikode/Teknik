using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.ViewModels
{
    public abstract class ViewModelBase
    {
        private Config _config;

        public Config Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
            }
        }
    }
}