using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Error.ViewModels
{
    public class ErrorViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Exception Exception { get; set; }
    }
}
