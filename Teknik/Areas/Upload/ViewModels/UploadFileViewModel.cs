using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Upload.ViewModels
{
    public class UploadFileViewModel
    {
        public string fileType { get; set; }

        public string fileExt { get; set; }

        public string iv { get; set; }

        public int keySize { get; set; }

        public int blockSize { get; set; }

        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public UploadSettings options { get; set; }

        public IFormFile file { get; set; }
    }
}
