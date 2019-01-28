using System.ComponentModel.DataAnnotations;
using Teknik.ViewModels;

namespace Teknik.Areas.Paste.ViewModels
{
    public class PasteEditViewModel : ViewModelBase
    {
        public string Url { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public string Syntax { get; set; }
    }
}
