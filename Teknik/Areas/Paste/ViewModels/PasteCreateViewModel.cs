using System.ComponentModel.DataAnnotations;
using Teknik.ViewModels;

namespace Teknik.Areas.Paste.ViewModels
{
    public class PasteCreateViewModel : ViewModelBase
    {
        [Required]
        public string Content { get; set; }

        public string Title { get; set; }

        public string Syntax { get; set; }

        [Range(1, int.MaxValue)]
        public int? ExpireLength { get; set; }

        public string ExpireUnit { get; set; }
        
        public string Password { get; set; }

        public string CurrentSub { get; set; }
    }
}
