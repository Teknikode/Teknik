using System;

namespace Teknik.IdentityServer.ViewModels
{
    public class ErrorViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusCode { get; set; }
        public Exception Exception { get; set; }
    }
}