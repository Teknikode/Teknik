using IdentityModel.Client;
using IdentityServer4.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Users.Utility
{
    public static class IdentityHelper
    {
        public static async Task GetAccessToken(this HttpClient client, Config config)
        {
            var token = await client.GetAccessToken(config.UserConfig.IdentityServerConfig.Authority, config.UserConfig.IdentityServerConfig.ClientId, config.UserConfig.IdentityServerConfig.ClientSecret, "auth-api");
            client.SetBearerToken(token);
        }

        public static async Task<string> GetAccessToken(this HttpClient client, string authority, string clientId, string secret, string scope)
        {
            var disco = await client.GetDiscoveryDocumentAsync(authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = secret,
                Scope = scope
            });

            if (tokenResponse.IsError) throw new Exception(tokenResponse.Error);

            return tokenResponse.AccessToken;
        }

        public static Uri CreateUrl(Config config, string path)
        {
            var authUrl = new Uri(config.UserConfig.IdentityServerConfig.Authority);
            return new Uri(authUrl, path);
        }

        public static async Task<IdentityResult> Get(Config config, Uri url)
        {
            var client = new HttpClient();
            await client.GetAccessToken(config);

            var content = await client.GetStringAsync(url);
            if (!string.IsNullOrEmpty(content))
            {
                return JsonConvert.DeserializeObject<IdentityResult>(content);
            }

            return new IdentityResult() { Success = false, Message = "No Data Received" };
        }

        public static async Task<IdentityResult> Post(Config config, Uri url, object data)
        {
            var client = new HttpClient();
            await client.GetAccessToken(config);


            var response = await client.PostAsJsonAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    return JsonConvert.DeserializeObject<IdentityResult>(content);
                }
                return new IdentityResult() { Success = false, Message = "No Data Received" };
            }
            return new IdentityResult() { Success = false, Message = "HTTP Error: " + response.StatusCode + " | " + (await response.Content.ReadAsStringAsync()) };
        }

        public static string[] GrantTypeToGrants(IdentityClientGrant grantType)
        {
            List<string> grants = new List<string>();
            switch (grantType)
            {
                case IdentityClientGrant.Implicit:
                    grants.Add(GrantType.Implicit);
                    break;
                case IdentityClientGrant.AuthorizationCode:
                    grants.Add(GrantType.AuthorizationCode);
                    break;
                case IdentityClientGrant.ClientCredentials:
                    grants.Add(GrantType.ClientCredentials);
                    break;
                default:
                    grants.Add(GrantType.Hybrid);
                    break;
            }
            return grants.ToArray();
        }

        public static IdentityClientGrant GrantsToGrantType(string[] grants)
        {
            if (grants.Contains(GrantType.Implicit))
            {
                return IdentityClientGrant.Implicit;
            }
            else if (grants.Contains(GrantType.AuthorizationCode))
            {
                return IdentityClientGrant.AuthorizationCode;
            }
            else if (grants.Contains(GrantType.ClientCredentials))
            {
                return IdentityClientGrant.ClientCredentials;
            }
            else
            {
                return IdentityClientGrant.ClientCredentials;
            }
        }

        // API Functions

        public static async Task<IdentityResult> CreateUser(Config config, string username, string password, string recoveryEmail)
        {
            var manageUrl = CreateUrl(config, $"Manage/CreateUser");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    password = password,
                    recoveryEmail = recoveryEmail
                });
            return response;
        }

        public static async Task<bool> DeleteUser(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/DeleteUser");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username
                });
            return response.Success;
        }

        public static async Task<bool> UserExists(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/UserExists?username={username}");

            var result = await Get(config, manageUrl);
            if (result.Success)
            {
                return (bool)result.Data;
            }
            throw new Exception(result.Message);
        }

        public static async Task<IdentityUserInfo> GetIdentityUserInfo(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/GetUserInfo?username={username}");

            var result = await Get(config, manageUrl);
            if (result.Success)
            {
                return new IdentityUserInfo((JObject)result.Data);
            }
            throw new Exception(result.Message);
        }

        public static async Task<bool> CheckPassword(Config config, string username, string password)
        {
            var manageUrl = CreateUrl(config, $"Manage/CheckPassword");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    password = password
                });
            if (response.Success)
            {
                return (bool)response.Data;
            }
            return false;
        }

        public static async Task<string> GeneratePasswordResetToken(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/GeneratePasswordResetToken");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username
                });
            if (response.Success)
            {
                return (string)response.Data;
            }
            throw new Exception(response.Message);
        }

        public static async Task<IdentityResult> ResetPassword(Config config, string username, string token, string newPassword)
        {
            var manageUrl = CreateUrl(config, $"Manage/ResetPassword");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    token = token,
                    password = newPassword
                });
            return response;
        }

        public static async Task<IdentityResult> UpdatePassword(Config config, string username, string currentPassword, string newPassword)
        {
            var manageUrl = CreateUrl(config, $"Manage/UpdatePassword");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    currentPassword = currentPassword,
                    newPassword = newPassword
                });
            return response;
        }

        public static async Task<string> UpdateRecoveryEmail(Config config, string username, string email)
        {
            var manageUrl = CreateUrl(config, $"Manage/UpdateEmail");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    email = email
                });
            if (response.Success)
            {
                return (string)response.Data;
            }
            throw new Exception(response.Message);
        }

        public static async Task<IdentityResult> VerifyRecoveryEmail(Config config, string username, string token)
        {
            var manageUrl = CreateUrl(config, $"Manage/VerifyEmail");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    token = token
                });
            return response;
        }

        public static async Task<IdentityResult> UpdateAccountStatus(Config config, string username, AccountStatus accountStatus)
        {
            var manageUrl = CreateUrl(config, $"Manage/UpdateAccountStatus");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    accountStatus = accountStatus
                });
            return response;
        }

        public static async Task<IdentityResult> UpdateAccountType(Config config, string username, AccountType accountType)
        {
            var manageUrl = CreateUrl(config, $"Manage/UpdateAccountType");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    accountType = accountType
                });
            return response;
        }

        public static async Task<IdentityResult> UpdatePGPPublicKey(Config config, string username, string publicKey)
        {
            var manageUrl = CreateUrl(config, $"Manage/UpdatePGPPublicKey");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    pgpPublicKey = publicKey
                });
            return response;
        }

        public static async Task<string> Get2FAKey(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/Get2FAKey?username={username}");

            var result = await Get(config, manageUrl);
            if (result.Success)
            {
                return (string)result.Data;
            }
            throw new Exception(result.Message);
        }

        public static async Task<string> Reset2FAKey(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/Reset2FAKey");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username
                });
            if (response.Success)
            {
                return (string)response.Data;
            }
            throw new Exception(response.Message);
        }

        public static async Task<string[]> Enable2FA(Config config, string username, string code)
        {
            var manageUrl = CreateUrl(config, $"Manage/Enable2FA");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    code = code
                });
            if (response.Success)
            {
                return ((JArray)response.Data).ToObject<string[]>();
            }
            throw new Exception(response.Message);
        }

        public static async Task<IdentityResult> Disable2FA(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/Disable2FA");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username
                });
            return response;
        }

        public static async Task<string[]> GenerateRecoveryCodes(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/GenerateRecoveryCodes");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username
                });
            if (response.Success)
            {
                return ((JArray)response.Data).ToObject<string[]>();
            }
            throw new Exception(response.Message);
        }

        public static async Task<Client> GetClient(Config config, string username, string clientId)
        {
            var manageUrl = CreateUrl(config, $"Manage/GetClient?username={username}&clientId={clientId}");

            var result = await Get(config, manageUrl);
            if (result.Success)
            {
                return ((JObject)result.Data).ToObject<Client>();
            }
            throw new Exception(result.Message);
        }

        public static async Task<Client[]> GetClients(Config config, string username)
        {
            var manageUrl = CreateUrl(config, $"Manage/GetClients?username={username}");

            var result = await Get(config, manageUrl);
            if (result.Success)
            {
                return ((JArray)result.Data).ToObject<Client[]>();
            }
            throw new Exception(result.Message);
        }

        public static async Task<IdentityResult> CreateClient(Config config, string username, string name, string homepageUrl, string logoUrl, string callbackUrl, string[] allowedGrants, string[] allowedScopes)
        {
            var manageUrl = CreateUrl(config, $"Manage/CreateClient");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    name = name,
                    homepageUrl = homepageUrl,
                    logoUrl = logoUrl,
                    callbackUrl = callbackUrl,
                    allowedScopes = allowedScopes,
                    allowedGrants = allowedGrants
                });
            return response;
        }

        public static async Task<IdentityResult> EditClient(Config config, string username, string clientId, string name, string homepageUrl, string logoUrl, string callbackUrl, string[] allowedGrants, string[] allowedScopes)
        {
            var manageUrl = CreateUrl(config, $"Manage/EditClient");

            var response = await Post(config, manageUrl,
                new
                {
                    username = username,
                    clientId = clientId,
                    name = name,
                    homepageUrl = homepageUrl,
                    logoUrl = logoUrl,
                    callbackUrl = callbackUrl,
                    allowedScopes = allowedScopes,
                    allowedGrants = allowedGrants
                });
            return response;
        }

        public static async Task<IdentityResult> DeleteClient(Config config, string clientId)
        {
            var manageUrl = CreateUrl(config, $"Manage/DeleteClient");

            var response = await Post(config, manageUrl,
                new
                {
                    clientId = clientId
                });
            return response;
        }
    }
}
