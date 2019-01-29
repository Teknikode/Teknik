using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreationDate { get; set; }

        public DateTime LastSeen { get; set; }

        public AccountType AccountType { get; set; }

        public AccountStatus AccountStatus { get; set; }

        public string PGPPublicKey { get; set; }

        public DateTime LastEdit { get; set; }

        public ApplicationUser() : base()
        {
            Init();
        }

        public ApplicationUser(string userName) : base(userName)
        {
            Init();
        }

        private void Init()
        {
            CreationDate = DateTime.Now;
            LastSeen = DateTime.Now;
            LastEdit = DateTime.Now;
            AccountType = AccountType.Basic;
            AccountStatus = AccountStatus.Active;
            PGPPublicKey = null;
        }

        public List<Claim> ToClaims()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("username", UserName));
            claims.Add(new Claim("creation-date", CreationDate.ToString("o")));
            claims.Add(new Claim("last-seen", LastSeen.ToString("o")));
            claims.Add(new Claim("last-edit", LastEdit.ToString("o")));
            claims.Add(new Claim("account-type", AccountType.ToString()));
            claims.Add(new Claim("account-status", AccountStatus.ToString()));
            claims.Add(new Claim("recovery-email", Email ?? string.Empty));
            claims.Add(new Claim("recovery-verified", EmailConfirmed.ToString()));
            claims.Add(new Claim("2fa-enabled", TwoFactorEnabled.ToString()));
            claims.Add(new Claim("pgp-public-key", PGPPublicKey ?? string.Empty));
            return claims;
        }

        public JObject ToJson()
        {
            return new JObject()
            {
                new JProperty("username", UserName),
                new JProperty("creation-date", CreationDate),
                new JProperty("last-seen", LastSeen),
                new JProperty("last-edit", LastEdit),
                new JProperty("account-type", AccountType),
                new JProperty("account-status", AccountStatus),
                new JProperty("recovery-email", Email),
                new JProperty("recovery-verified", EmailConfirmed),
                new JProperty("2fa-enabled", TwoFactorEnabled),
                new JProperty("pgp-public-key", PGPPublicKey)
            };
        }
    }
}
