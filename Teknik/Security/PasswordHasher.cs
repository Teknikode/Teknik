using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;

namespace Teknik.Security
{
    public class PasswordHasher : IPasswordHasher<User>
    {
        private readonly Config _config;

        public PasswordHasher(Config config)
        {
            _config = config;
        }

        public string HashPassword(User user, string password)
        {
            return UserHelper.GeneratePassword(_config, user, password);
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            var hashedProvidedPassword = UserHelper.GeneratePassword(_config, user, providedPassword);
            if (hashedPassword == hashedProvidedPassword)
            {
                return PasswordVerificationResult.Success;
            }
            return PasswordVerificationResult.Failed;
        }
    }
}
