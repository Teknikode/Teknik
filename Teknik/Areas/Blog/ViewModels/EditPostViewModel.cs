using Teknik.ViewModels;

namespace Teknik.Areas.Blog.ViewModels
{
    public class EditPostViewModel : ViewModelBase
    {
        public int PostId { get; set; }

        public string Title { get; set; }
        
        public string Article { get; set; }
    }
}