using Teknik.ViewModels;

namespace Teknik.Areas.Error.ViewModels
{
    public class SubmitReportViewModel : ViewModelBase
    {
        public string Message { get; set; }
        
        public string Exception { get; set; }
        
        public string CurrentUrl { get; set; }
    }
}
