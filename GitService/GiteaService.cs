using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Teknik.GitService
{
    public class GiteaService : IGitService
    {
        private readonly int _sourceId;
        private readonly string _host;
        private readonly string _accessToken;

        private readonly string _server;
        private readonly string _database;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;

        public GiteaService(int sourceId, string host, string accessToken, string server, string database, string username, string password, int port)
        {
            _sourceId = sourceId;
            _host = host;
            _accessToken = accessToken;

            _server = server;
            _database = database;
            _username = username;
            _password = password;
            _port = port;
        }

        public bool AccountExists(string username)
        {
            Uri baseUri = new Uri(_host);
            Uri finalUri = new Uri(baseUri, "api/v1/users/" + username + "?token=" + _accessToken);
            WebRequest request = WebRequest.Create(finalUri);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public void CreateAccount(string username, string email, string password)
        {
            // Add gogs user
            using (var client = new WebClient())
            {
                var obj = new { source_id = _sourceId, username = username, email = email, login_name = email, password = password };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                Uri baseUri = new Uri(_host);
                Uri finalUri = new Uri(baseUri, "api/v1/admin/users?token=" + _accessToken);
                string result = client.UploadString(finalUri, "POST", json);
            }
        }

        public void DeleteAccount(string username)
        {
            try
            {
                Uri baseUri = new Uri(_host);
                Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + username + "?token=" + _accessToken);
                WebRequest request = WebRequest.Create(finalUri);
                request.Method = "DELETE";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.NotFound && response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception("Response Code: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // This error signifies the user doesn't exist, so we can continue deleting
                if (ex.Message != "The remote server returned an error: (404) Not Found.")
                {
                    throw new Exception("Unable to delete git account.  Exception: " + ex.Message);
                }
            }
        }

        public void EditPassword(string username, string email, string password)
        {
            using (var client = new WebClient())
            {
                var obj = new { source_id = _sourceId, email = email, login_name = email, password = password };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                Uri baseUri = new Uri(_host);
                Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + username + "?token=" + _accessToken);
                string result = client.UploadString(finalUri, "PATCH", json);
            }
        }

        public void EnableAccount(string username, string email)
        {
            ChangeAccountStatus(username, email, true);
        }

        public void DisableAccount(string username, string email)
        {
            ChangeAccountStatus(username, email, false);
        }

        public void ChangeAccountStatus(string username, string email, bool active)
        {
            using (var client = new WebClient())
            {
                var obj = new { active = active, email = email };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                Uri baseUri = new Uri(_host);
                Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + username + "?token=" + _accessToken);
                string result = client.UploadString(finalUri, "PATCH", json);
            }
        }

        public DateTime LastActive(string email)
        {
            // We need to check the actual git database
            MysqlDatabase mySQL = new MysqlDatabase(_server, _database, _username, _password, _port);
            string sql = @"SELECT 
	                                CASE
		                                WHEN MAX(gogs.action.created) >= MAX(gogs.user.updated) THEN MAX(gogs.action.created)
		                                WHEN MAX(gogs.user.updated) >= MAX(gogs.action.created) THEN MAX(gogs.user.updated)
		                                ELSE MAX(gogs.user.updated)
	                                END AS LastUpdate
                                FROM gogs.user
                                LEFT JOIN gogs.action ON gogs.user.id = gogs.action.act_user_id
                                WHERE gogs.user.login_name = {0}";
            var results = mySQL.Query(sql, new object[] { email });

            DateTime lastActive = new DateTime(1, 0, 0);
            if (results != null && results.Any())
            {
                var result = results.First();
                DateTime.TryParse(result["LastUpdate"].ToString(), out lastActive);
            }
            return lastActive;
        }
    }
}
