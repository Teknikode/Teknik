using Teknik.ViewModels;

namespace Teknik.Areas.Blog.ViewModels
{
    public class CreatePostViewModel : ViewModelBase
    {
        public int BlogId { get; set; }

        public string Title { get; set; }
        
        public string Article { get; set; }
    }
}