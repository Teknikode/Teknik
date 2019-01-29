using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;
using Microsoft.Extensions.Identity.Core;
using Microsoft.Extensions.Options;
using Teknik.Utilities.Cryptography;
using Teknik.Utilities;
using System.Text;
using Teknik.IdentityServer.Models;

namespace Teknik.IdentityServer.Security
{
    public class TeknikPasswordHasher : PasswordHasher<ApplicationUser>
    {
        private readonly Config _config;

        public TeknikPasswordHasher(Config config)
        {
            _config = config;
        }

        public override PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            // Test legacy password hashes
            #region Legacy Checks
            byte[] hashBytes = SHA384.Hash(user.UserName.ToLower(), providedPassword);
            string hash = hashBytes.ToHex();
            if (hashedPassword == hash)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            hash = Encoding.ASCII.GetString(hashBytes);
            if (hashedPassword == hash)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            hash = SHA256.Hash(providedPassword, _config.Salt1, _config.Salt2);
            if (hashedPassword == hash)
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }
            #endregion

            // Test Latest
            return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }
}
