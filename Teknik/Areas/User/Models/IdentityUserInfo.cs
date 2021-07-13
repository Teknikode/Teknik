using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.Areas.Users.Models
{
    public class IdentityUserInfo
    {
        public DateTime? CreationDate { get; set; }

        public DateTime? LastSeen { get; set; }

        public AccountType? AccountType { get; set; }

        public AccountStatus? AccountStatus { get; set; }

        public string RecoveryEmail { get; set; }

        public bool? RecoveryVerified { get; set; }

        public bool? TwoFactorEnabled { get; set; }

        public string PGPPublicKey { get; set; }

        public IdentityUserInfo() { }

        public IdentityUserInfo(IEnumerable<Claim> claims)
        {
            if (claims.FirstOrDefault(c => c.Type == "creation-date") != null)
            {
                if (DateTime.TryParse(claims.FirstOrDefault(c => c.Type == "creation-date").Value, out var dateTime))
                    CreationDate = dateTime;
            }
            if (claims.FirstOrDefault(c => c.Type == "last-seen") != null)
            {
                if (DateTime.TryParse(claims.FirstOrDefault(c => c.Type == "last-seen").Value, out var dateTime))
                    CreationDate = dateTime;
            }
            if (claims.FirstOrDefault(c => c.Type == "account-type") != null)
            {
                if (Enum.TryParse(claims.FirstOrDefault(c => c.Type == "account-type").Value, out AccountType accountType))
                    AccountType = accountType;
            }
            if (claims.FirstOrDefault(c => c.Type == "account-status") != null)
            {
                if (Enum.TryParse(claims.FirstOrDefault(c => c.Type == "account-status").Value, out AccountStatus accountStatus))
                    AccountStatus = accountStatus;
            }
            if (claims.FirstOrDefault(c => c.Type == "recovery-email") != null)
            {
                RecoveryEmail = claims.FirstOrDefault(c => c.Type == "recovery-email").Value;
            }
            if (claims.FirstOrDefault(c => c.Type == "recovery-verified") != null)
            {
                if (bool.TryParse(claims.FirstOrDefault(c => c.Type == "recovery-verified").Value, out var verified))
                    RecoveryVerified = verified;
            }
            if (claims.FirstOrDefault(c => c.Type == "2fa-enabled") != null)
            {
                if (bool.TryParse(claims.FirstOrDefault(c => c.Type == "2fa-enabled").Value, out var twoFactor))
                    TwoFactorEnabled = twoFactor;
            }
            if (claims.FirstOrDefault(c => c.Type == "pgp-public-key") != null)
            {
                PGPPublicKey = claims.FirstOrDefault(c => c.Type == "pgp-public-key").Value;
            }
        }

        public IdentityUserInfo(JObject info)
        {
            if (info["creation-date"] != null)
            {
                if (DateTime.TryParse(info["creation-date"].ToString(), out var dateTime))
                    CreationDate = dateTime;
            }
            if (info["last-seen"] != null)
            {
                if (DateTime.TryParse(info["last-seen"].ToString(), out var dateTime))
                    LastSeen = dateTime;
            }
            if (info["account-type"] != null)
            {
                if (Enum.TryParse(info["account-type"].ToString(), out AccountType accountType))
                    AccountType = accountType;
            }
            if (info["account-status"] != null)
            {
                if (Enum.TryParse(info["account-status"].ToString(), out AccountStatus accountStatus))
                    AccountStatus = accountStatus;
            }
            if (info["recovery-email"] != null)
            {
                RecoveryEmail = info["recovery-email"].ToString();
            }
            if (info["recovery-verified"] != null)
            {
                if (bool.TryParse(info["recovery-verified"].ToString(), out var verified))
                    RecoveryVerified = verified;
            }
            if (info["2fa-enabled"] != null)
            {
                if (bool.TryParse(info["2fa-enabled"].ToString(), out var twoFactor))
                    TwoFactorEnabled = twoFactor;
            }
            if (info["pgp-public-key"] != null)
            {
                PGPPublicKey = info["pgp-public-key"].ToString();
            }
        }
    }
}
