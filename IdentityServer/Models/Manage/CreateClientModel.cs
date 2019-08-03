using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.IdentityServer.Models.Manage
{
    public class CreateClientModel
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string HomepageUrl { get; set; }
        public string LogoUrl { get; set; }
        public string CallbackUrl { get; set; }
        public ICollection<string> AllowedScopes { get; set; }
        public ICollection<string> AllowedGrants { get; set; }
    }
}
