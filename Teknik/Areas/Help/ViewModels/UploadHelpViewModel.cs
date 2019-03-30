using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Help.ViewModels
{
    public class UploadHelpViewModel : ViewModelBase
    {
        public long MaxUploadSize { get; set; }
    }
}
