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
                DateTime dateTime = new DateTime();
                if (DateTime.TryParse(claims.FirstOrDefault(c => c.Type == "creation-date").Value, out dateTime))
                    CreationDate = dateTime;
            }
            if (claims.FirstOrDefault(c => c.Type == "last-seen") != null)
            {
                DateTime dateTime = new DateTime();
                if (DateTime.TryParse(claims.FirstOrDefault(c => c.Type == "last-seen").Value, out dateTime))
                    CreationDate = dateTime;
            }
            if (claims.FirstOrDefault(c => c.Type == "account-type") != null)
            {
                AccountType accountType = Utilities.AccountType.Basic;
                if (Enum.TryParse(claims.FirstOrDefault(c => c.Type == "account-type").Value, out accountType))
                    AccountType = accountType;
            }
            if (claims.FirstOrDefault(c => c.Type == "account-status") != null)
            {
                AccountStatus accountStatus = Utilities.AccountStatus.Active;
                if (Enum.TryParse(claims.FirstOrDefault(c => c.Type == "account-status").Value, out accountStatus))
                    AccountStatus = accountStatus;
            }
            if (claims.FirstOrDefault(c => c.Type == "recovery-email") != null)
            {
                RecoveryEmail = claims.FirstOrDefault(c => c.Type == "recovery-email").Value;
            }
            if (claims.FirstOrDefault(c => c.Type == "recovery-verified") != null)
            {
                bool verified = false;
                if (bool.TryParse(claims.FirstOrDefault(c => c.Type == "recovery-verified").Value, out verified))
                    RecoveryVerified = verified;
            }
            if (claims.FirstOrDefault(c => c.Type == "2fa-enabled") != null)
            {
                bool twoFactor = false;
                if (bool.TryParse(claims.FirstOrDefault(c => c.Type == "2fa-enabled").Value, out twoFactor))
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
                DateTime dateTime = new DateTime();
                if (DateTime.TryParse(info["creation-date"].ToString(), out dateTime))
                    CreationDate = dateTime;
            }
            if (info["last-seen"] != null)
            {
                DateTime dateTime = new DateTime();
                if (DateTime.TryParse(info["last-seen"].ToString(), out dateTime))
                    LastSeen = dateTime;
            }
            if (info["account-type"] != null)
            {
                AccountType accountType = Utilities.AccountType.Basic;
                if (Enum.TryParse(info["account-type"].ToString(), out accountType))
                    AccountType = accountType;
            }
            if (info["account-status"] != null)
            {
                AccountStatus accountStatus = Utilities.AccountStatus.Active;
                if (Enum.TryParse(info["account-status"].ToString(), out accountStatus))
                    AccountStatus = accountStatus;
            }
            if (info["recovery-email"] != null)
            {
                RecoveryEmail = info["recovery-email"].ToString();
            }
            if (info["recovery-verified"] != null)
            {
                bool verified = false;
                if (bool.TryParse(info["recovery-verified"].ToString(), out verified))
                    RecoveryVerified = verified;
            }
            if (info["2fa-enabled"] != null)
            {
                bool twoFactor = false;
                if (bool.TryParse(info["2fa-enabled"].ToString(), out twoFactor))
                    TwoFactorEnabled = twoFactor;
            }
            if (info["pgp-public-key"] != null)
            {
                PGPPublicKey = info["pgp-public-key"].ToString();
            }
        }
    }
}
