using IRCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IRCSharp.Messaging;
using Microsoft.AspNet.SignalR;
using Teknik.Hubs;
using Microsoft.AspNet.SignalR.Hubs;
using Teknik.Configuration;
using System.Net;
using Teknik.Utilities;

namespace Teknik.SignalR
{
    public class IRCClient
    {
        private IRC _IRC;
        private dynamic _CallerContext;

        // State Variables
        private bool _Connected = false;
        public bool Connected
        {
            get
            {
                return _Connected;
            }
            set
            {
                _Connected = value;
            }
        }

        public IRCClient(dynamic callerContext)
        {
            Config config = Config.Load();
            _CallerContext = callerContext;

            _IRC = new IRC(config.IRCConfig.MaxMessageLength);

            _IRC.ConnectEvent += HandleConnectEvent;
            _IRC.DisconnectEvent += HandleDisconnectEvent;
            _IRC.ExceptionThrown += HandleIrcExceptionThrown;

            _IRC.Message.RawMessageEvent += Message_RawMessageEvent;
        }

        public void Connect(string username)
        {
            if (!_Connected)
            {
                Config config = Config.Load();
                IPAddress[] ipList = Dns.GetHostAddresses(config.IRCConfig.Host);
                foreach (IPAddress ip in ipList)
                {
                    _Connected = _IRC.Connect(ip, config.IRCConfig.Port);
                    if (_Connected)
                    {
                        break;
                    }
                }

                if (_Connected)
                {
                    _IRC.Login("Teknik-WebClient", new Nick()
                    {
                        Nickname = username,
                        Host = Dns.GetHostName(),
                        Realname = username,
                        Username = username
                    });
                }
            }
        }

        public void Disconnect()
        {
            if (_Connected)
            {
                _IRC.Disconnect();
            }
        }

        public void SendRawMessage(string message)
        {
            if (_Connected)
            {
                _IRC.Command.SendRaw(message);
            }
        }

        private void Message_RawMessageEvent(object sender, string message)
        {
            _CallerContext.rawMessageReceived(message);
        }

        private void HandleIrcExceptionThrown(Exception obj)
        {
            _CallerContext.exception(obj.GetFullMessage(true));
        }

        private void HandleDisconnectEvent()
        {
            _CallerContext.disconnected();
        }

        private void HandleConnectEvent()
        {
            _CallerContext.connected();
        }
    }
}