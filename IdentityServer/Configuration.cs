using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Teknik.Configuration;

namespace Teknik.IdentityServer.Configuration
{
    internal class Clients
    {
        public static IEnumerable<Client> Get(Config config)
        {
            return new List<Client> {
                new Client
                {
                    ClientId = config.UserConfig.IdentityServerConfig.ClientId,
                    ClientName = "Teknik Web Services",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets =
                    {
                        new Secret(config.UserConfig.IdentityServerConfig.ClientSecret.Sha256())
                    },

                    RequireConsent = false,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "role",
                        "account-info",
                        "security-info",
                        "teknik-api.read",
                        "teknik-api.write",
                        "auth-api"
                    },
                    AllowOfflineAccess = true
                }
            };
        }
    }

    internal class Resources
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResource
                {
                    Name = "account-info",
                    DisplayName = "Account Info",
                    UserClaims = new List<string>
                    {
                        "username",
                        "email",
                        "creation-date",
                        "last-seen",
                        "account-type",
                        "account-status"
                    }
                },
                new IdentityResource
                {
                    Name = "security-info",
                    DisplayName = "Security Info",
                    UserClaims = new List<string>
                    {
                        "recovery-email",
                        "recovery-verified",
                        "pgp-public-key"
                    }
                },
                new IdentityResource {
                    Name = "role",
                    DisplayName = "Role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>()
            {
                new ApiScope(name: "teknik-api.read", displayName: "Teknik API Read Access"),
                new ApiScope(name: "teknik-api.write", displayName: "Teknik API Write Access"),
                new ApiScope(name: "auth-api") { Required = true, ShowInDiscoveryDocument = false }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources(Config config)
        {
            return new List<ApiResource>() 
            {
                new ApiResource 
                {
                    Name = config.UserConfig.IdentityServerConfig.APIName,
                    DisplayName = "Teknik API",
                    Description = "Teknik API Access for end users",
                    UserClaims = new List<string> {"role", "username"},
                    ApiSecrets = new List<Secret> {new Secret(config.UserConfig.IdentityServerConfig.APISecret.Sha256()) },
                    Scopes = new List<string> { "teknik-api.read", "teknik-api.write" }
                },
                new ApiResource {
                    Name = "auth-api",
                    DisplayName = "Auth Server API",
                    Description = "Auth Server API Access for managing the Auth Server",
                    Scopes = new List<string> { "auth-api" }
                }
            };
        }
    }

    internal class Policies
    {
        public static IEnumerable<Policy> Get()
        {
            return new List<Policy>()
            {
                new Policy
                {
                    Name = "Internal",
                    Scopes = { "auth-api" }
                }
            };
        }
    }

    internal class Policy
    {
        public string Name { get; set; }
        public ICollection<string> Scopes { get; set; }

        public Policy()
        {
            Name = string.Empty;
            Scopes = new List<string>();
        }
    }
}