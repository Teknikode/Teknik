using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.ViewModels
{
    public class ViewModelBase
    {
        public bool Error { get; set; }

        public string ErrorMessage { get; set; }

        public ViewModelBase()
        {
            Error = false;
            ErrorMessage = string.Empty;
        }
    }
}
