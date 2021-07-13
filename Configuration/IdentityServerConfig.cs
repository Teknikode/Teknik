using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Configuration
{
    public class IdentityServerConfig
    {
        public string Host { get; set; }
        public string Authority { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> RedirectUris { get; set; }
        public List<string> PostLogoutRedirectUris { get; set; }
        public List<string> AllowedCorsOrigins { get; set; }

        public string APIName { get; set; }
        public string APISecret { get; set; }

        public string SigningCertificate { get; set; }

        public IdentityServerConfig()
        {
            Host = "localhost:5002";
            Authority = "https://localhost:5002";
            ClientId = "mvc.client";
            ClientSecret = "mysecret";
            RedirectUris = new List<string>();
            PostLogoutRedirectUris = new List<string>();
            AllowedCorsOrigins = new List<string>();
            APIName = "api";
            APISecret = "secret";
        }
    }
}
