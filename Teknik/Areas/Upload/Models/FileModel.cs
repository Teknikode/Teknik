using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Upload.Models
{
    public class FileModel
    {
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string IV { get; set; }

        public string ContentAsBase64String { get; set; }
    }

}
