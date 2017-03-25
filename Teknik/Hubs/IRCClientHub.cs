using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Attributes;
using Teknik.SignalR;
using System.Threading.Tasks;

namespace Teknik.Hubs
{
    [HubName("ircClient")]
    [TeknikAuthorize]
    public class IRCClientHub : Hub
    {
        private static Dictionary<string, IRCClient> _Clients = new Dictionary<string, IRCClient>();

        public IRCClientHub()
        {
        }

        public override Task OnConnected()
        {
            if (Context != null)
            {
                if (!_Clients.ContainsKey(Context.ConnectionId))
                {
                    if (Context.User.Identity.IsAuthenticated)
                    {
                        // Add the new connection to the main list
                        _Clients.Add(Context.ConnectionId, new IRCClient(Clients.Caller));

                        // Auto Connect to the server at startup
                        Connect();
                    }
                }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (_Clients.ContainsKey(Context.ConnectionId))
            {
                // Disconnect from the IRC session
                _Clients[Context.ConnectionId].Disconnect();

                // Remove the irc client
                _Clients.Remove(Context.ConnectionId);
            }
            return base.OnDisconnected(stopCalled);
        }

        public void Connect()
        {
            if (_Clients.ContainsKey(Context.ConnectionId))
            {
                _Clients[Context.ConnectionId].Connect(Context.User.Identity.Name);
            }
        }

        public void SendRawMessage(string message)
        {
            if (_Clients.ContainsKey(Context.ConnectionId))
            {
                _Clients[Context.ConnectionId].SendRawMessage(message);
            }
        }
    }
}