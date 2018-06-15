using System;
using Teknik.ViewModels;

namespace Teknik.Areas.Error.ViewModels
{
    public class ErrorViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusCode { get; set; }
        public Exception Exception { get; set; }
    }
}
