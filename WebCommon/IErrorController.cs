using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.WebCommon
{
    public interface IErrorController
    {
        public ControllerContext ControllerContext { get; set; }

        public IActionResult HttpError(int statusCode, Exception exception);
    }
}
