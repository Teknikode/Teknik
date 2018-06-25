using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Utilities;

namespace Teknik.Security
{
    public class UserStore : IUserStore<User>, IUserLoginStore<User>, IUserClaimStore<User>, IUserPasswordStore<User>, IUserRoleStore<User>, IQueryableUserStore<User>
    {
        private readonly TeknikEntities _dbContext;
        private readonly Config _config;

        public IQueryable<User> Users => _dbContext.Users.AsQueryable();

        public UserStore(TeknikEntities dbContext, Config config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            UserHelper.AddAccount(_dbContext, _config, user, user.Password);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            UserHelper.DeleteAccount(_dbContext, _config, user);
            return IdentityResult.Success;
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return UserHelper.GetUser(_dbContext, userId);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return UserHelper.GetUser(_dbContext, normalizedUserName);
        }

        public async Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return user.Username;
        }

        public async Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return user.Username;
        }

        public async Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return user.Username;
        }

        public async Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.Username = normalizedName;
            UserHelper.EditUser(_dbContext, _config, user);
        }

        public async Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.Username = userName;
            UserHelper.EditUser(_dbContext, _config, user);
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            UserHelper.EditUser(_dbContext, _config, user);
            return IdentityResult.Success;
        }

        // Password Store
        public async Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.HashedPassword = passwordHash;
            UserHelper.EditUser(_dbContext, _config, user);
        }

        public async Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return user.HashedPassword;
        }

        public async Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return !string.IsNullOrEmpty(user.HashedPassword);
        }

        // Role Store
        public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = _dbContext.Roles.Where(r => r.Name == roleName).FirstOrDefault();
            if (role == null)
                throw new ArgumentException("Role does not exist", "roleName");

            bool alreadyHasRole = await IsInRoleAsync(user, roleName, cancellationToken);
            if (!alreadyHasRole)
            {
                UserRole userRole = new UserRole();
                userRole.Role = role;
                userRole.User = user;
                await _dbContext.UserRoles.AddAsync(userRole);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var userRoles = user.UserRoles.Where(r => r.Role.Name == roleName).ToList();
            if (userRoles != null)
            {
                foreach (var userRole in userRoles)
                {
                    _dbContext.UserRoles.Remove(userRole);
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            return user.UserRoles.Select(ur => ur.Role.Name).ToList();
        }

        public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            return UserHelper.UserHasRoles(user, roleName);
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var userRoles = _dbContext.UserRoles.Where(r => r.Role.Name == roleName).ToList();
            if (userRoles != null)
            {
                return userRoles.Select(ur => ur.User).ToList();
            }
            return new List<User>();
        }

        // Login Info Store
        public async Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            LoginInfo info = new LoginInfo();
            info.LoginProvider = login.LoginProvider;
            info.ProviderDisplayName = login.ProviderDisplayName;
            info.ProviderKey = login.ProviderKey;
            info.User = user;

            await _dbContext.UserLogins.AddAsync(info);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var logins = user.Logins.Where(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey).ToList();
            if (logins != null)
            {
                foreach (var login in logins)
                {
                    _dbContext.UserLogins.Remove(login);
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            List<UserLoginInfo> logins = new List<UserLoginInfo>();
            foreach (var login in user.Logins)
            {
                UserLoginInfo info = new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName);
                logins.Add(info);
            }
            return logins;
        }

        public async Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var foundLogin = _dbContext.UserLogins.Where(ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey).FirstOrDefault();
            if (foundLogin != null)
            {
                return foundLogin.User;
            }
            return null;
        }

        // Claim Store
        public async Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Add their roles
            foreach (var role in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            }

            return claims;
        }

        public async Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                if (claim.Type == ClaimTypes.Role)
                {
                    await AddToRoleAsync(user, claim.Value, cancellationToken);
                }
            }
        }

        public async Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            // "no"
        }

        public async Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            // "no"
        }

        public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            List<User> foundUsers = new List<User>();
            if (claim.Type == ClaimTypes.Role)
            {
                var users = await GetUsersInRoleAsync(claim.Value, cancellationToken);
                if (users != null && users.Any())
                    foundUsers.AddRange(users);
            }
            else if (claim.Type == ClaimTypes.Name)
            {
                var user = await FindByIdAsync(claim.Value, cancellationToken);
                if (user != null)
                    foundUsers.Add(user);
            }
            return foundUsers;
        }
    }
}
