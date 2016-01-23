using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Paste.ViewModels
{
    public class PasswordViewModel : ViewModelBase
    {
        public string Url { get; set; }

        public string CallingAction { get; set; }
    }
}
