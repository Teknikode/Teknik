using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Attributes;
using Teknik.SignalR;
using System.Threading.Tasks;
using System.Timers;
using Teknik.Areas.Users.Utility;
using Teknik.Areas.Users.Models;
using Teknik.Models;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Hubs
{
    [HubName("ircClient")]
    public class IRCClientHub : Hub
    {
        private static Dictionary<string, string> _Connections = new Dictionary<string, string>();
        private static Dictionary<string, Tuple<IRCClient, Timer>> _Clients = new Dictionary<string, Tuple<IRCClient, Timer>>();
        private const int _DisconnectTimeout = 30; // Timeout to disconnect from IRC server after client disconnects from hub (Seconds)

        public IRCClientHub()
        {
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Get the username for this connection
            string username = string.Empty;
            if (Context != null)
            {
                if (_Connections.ContainsKey(Context.ConnectionId))
                {
                    // Get the username
                    username = _Connections[Context.ConnectionId];

                    // Remove this connection from the list
                    _Connections.Remove(Context.ConnectionId);
                }
            }

            // Start disconnect timer for this user
            if (_Clients.ContainsKey(username))
            {
                // Start disconnect timer
                _Clients[username].Item2.Start();
            }

            return base.OnDisconnected(stopCalled);
        }

        public string Connect()
        {            
            // Create guest username
            string username = "Guest_" + StringHelper.RandomString(6, "0123456789");
            Connect(username, string.Empty);

            return username;
        }

        public bool Connect(string username, string password)
        {
            bool success = true;
            // If the password is supplied, verify the password
            if (!string.IsNullOrEmpty(password))
            {
                TeknikEntities db = new TeknikEntities();
                User user = UserHelper.GetUser(db, username);
                if (user != null)
                {
                    Config config = Config.Load();
                    success = UserHelper.UserPasswordCorrect(db, config, user, password);
                }
                else
                {
                    success = false;
                }
            }

            if (success)
            {
                // Update this connection with the username associated we want to associate with it
                if (Context != null)
                {
                    if (!_Connections.ContainsKey(Context.ConnectionId))
                    {
                        _Connections.Add(Context.ConnectionId, string.Empty);
                    }
                    _Connections[Context.ConnectionId] = username;
                }

                // Add the client for this user if it doesn't exist
                if (!_Clients.ContainsKey(username))
                {
                    // Client doesn't exist, so create it
                    Timer disconnectTimer = new Timer(_DisconnectTimeout * 1000);
                    disconnectTimer.Elapsed += (sender, e) => DisconnectTimer_Elapsed(sender, e, username);

                    // Add the new connection to the main list
                    _Clients.Add(username, new Tuple<IRCClient, Timer>(new IRCClient(Clients.Caller), disconnectTimer));
                }

                // Stop the disconnect timer because we have reconnected
                _Clients[username].Item2.Stop();

                // Reassociate this connection with the user
                _Clients[username].Item1.CallerContext = Clients.Caller;

                // Connect
                _Clients[username].Item1.Connect(username, password);
            }

            return success;
        }

        public void SendMessage(string location, string message)
        {
            string username = GetConnectionUser();
            if (_Clients.ContainsKey(username))
            {
                _Clients[username].Item1.SendMessage(location, message);
            }
        }

        public void SendRawMessage(string message)
        {
            string username = GetConnectionUser();
            if (_Clients.ContainsKey(username))
            {
                _Clients[username].Item1.SendRawMessage(message);
            }
        }

        private void DisconnectTimer_Elapsed(object sender, ElapsedEventArgs e, string username)
        {
            if (_Clients.ContainsKey(username))
            {
                // Disconnect from the IRC session
                _Clients[username].Item1.Disconnect();

                // Remove the irc client
                _Clients.Remove(username);
            }
        }

        private string GetConnectionUser()
        {
            if (Context != null)
            {
                if (_Connections.ContainsKey(Context.ConnectionId))
                {
                    return _Connections[Context.ConnectionId];
                }
            }
            return string.Empty;
        }
    }
}