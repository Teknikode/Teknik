using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.API.V1.Models
{
    public class GetAccessTokenAPIv1Model : BaseAPIv1Model
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
    }
}
