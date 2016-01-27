using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Transparency.ViewModels
{
    public class TransparencyViewModel : ViewModelBase
    {
        public int UploadCount { get; set; }

        public int PasteCount { get; set; }

        public int UserCount { get; set; }
    }
}
