using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Transparency.Models
{
    public class OneTime : Transaction
    {
        public string Recipient { get; set; }
    }
}
