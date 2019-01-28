using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.IdentityServer.Security
{
    public class TeknikRedirectUriValidator : IRedirectUriValidator
    {
        private readonly Config _config;

        public TeknikRedirectUriValidator(Config config)
        {
            _config = config;
        }

        public async Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.PostLogoutRedirectUris != null && client.PostLogoutRedirectUris.Any())
            {
                if (client.PostLogoutRedirectUris.Contains(requestedUri))
                    return true;
            }

            // Add special case for pre-configured redirect URIs
            if (client.ClientId == _config.UserConfig.IdentityServerConfig.ClientId && 
                _config.UserConfig.IdentityServerConfig.PostLogoutRedirectUris != null && 
                _config.UserConfig.IdentityServerConfig.PostLogoutRedirectUris.Any())
            {
                if (_config.UserConfig.IdentityServerConfig.PostLogoutRedirectUris.Contains(requestedUri))
                    return true;
            }

            return false;
        }

        public async Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.RedirectUris != null && client.RedirectUris.Any())
            {
                if (client.RedirectUris.Contains(requestedUri))
                    return true;
            }

            // Add special case for pre-configured redirect URIs
            if (client.ClientId == _config.UserConfig.IdentityServerConfig.ClientId && 
                _config.UserConfig.IdentityServerConfig.RedirectUris != null && 
                _config.UserConfig.IdentityServerConfig.RedirectUris.Any())
            {
                if (_config.UserConfig.IdentityServerConfig.RedirectUris.Contains(requestedUri))
                    return true;
            }

            return false;
        }
    }
}
