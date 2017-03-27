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
using System.Timers;
using IRCSharp.Commanding;

namespace Teknik.SignalR
{
    public class IRCClient
    {
        private IRC _IRC;
        private dynamic _CallerContext;
        public dynamic CallerContext
        {
            get
            {
                return _CallerContext;
            }
            set
            {
                _CallerContext = value;
            }
        }

        private bool _Guest;

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
            _Guest = false;

            Config config = Config.Load();
            _CallerContext = callerContext;

            _IRC = new IRC(config.IRCConfig.MaxMessageLength);

            _IRC.ConnectEvent += HandleConnectEvent;
            _IRC.DisconnectEvent += HandleDisconnectEvent;
            _IRC.ExceptionThrown += HandleIrcExceptionThrown;

            // Handle Message Events
            _IRC.Message.RawMessageEvent += Message_RawMessageEvent;

            _IRC.Message.ChannelMessageReceivedEvent += Message_ChannelMessageReceivedEvent;
            _IRC.Message.ChannelNoticeReceivedEvent += Message_ChannelNoticeReceivedEvent;
            _IRC.Message.ChannelModeChangeEvent += Message_ChannelModeChangeEvent;

            _IRC.Message.PrivateMessageReceivedEvent += Message_PrivateMessageReceivedEvent;
            _IRC.Message.PrivateNoticeReceivedEvent += Message_PrivateNoticeReceivedEvent;

            _IRC.Message.CTCPMessageReceivedEvent += Message_CTCPMessageReceivedEvent;
            _IRC.Message.CTCPNoticeReceivedEvent += Message_CTCPNoticeReceivedEvent;

            _IRC.Message.JoinChannelEvent += Message_JoinChannelEvent;
            _IRC.Message.InviteChannelEvent += Message_InviteChannelEvent;
            _IRC.Message.PartChannelEvent += Message_PartChannelEvent;
            _IRC.Message.QuitEvent += Message_QuitEvent;
            _IRC.Message.KickEvent += Message_KickEvent;
            _IRC.Message.TopicChangeEvent += Message_TopicChangeEvent;
            _IRC.Message.UserModeChangeEvent += Message_UserModeChangeEvent;
            _IRC.Message.NickChangeEvent += Message_NickChangeEvent;

            _IRC.Message.ServerReplyEvent += Message_ServerReplyEvent;

            // Handle Command Events
            _IRC.Command.PrivateMessageCommandEvent += Command_PrivateMessageCommandEvent;
            _IRC.Command.PrivateNoticeCommandEvent += Command_PrivateNoticeCommandEvent;
        }

        public void Connect()
        {
            // Create guest username
            string username = "Guest_" + StringHelper.RandomString(6, "0123456789");
            Connect(username, string.Empty);
        }

        public void Connect(string username, string password)
        {
            _Guest = string.IsNullOrEmpty(password);
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
                    // Login
                    _IRC.Login(config.Title + "-WebClient", new Nick()
                    {
                        Nickname = username,
                        Host = config.Host,
                        Realname = username,
                        Username = username
                    });

                    // Fire off event about current nick
                    _CallerContext.nickChanged(username);

                    // Try to identify if not guest
                    if (!_Guest)
                    {
                        // Identify to NickServ if need be
                        _IRC.Command.SendPrivateMessage("NickServ", string.Format("IDENTIFY {0}", password));
                    }
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

        /// <summary>
        /// Process the message and for commands, execute their command, otherwise send a private message
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string location, string message)
        {
            if (_Connected)
            {
                // Process Command
                if (message.StartsWith("/"))
                {
                    string[] msgSplit = message.Split(' ');
                    string cmd = msgSplit[0].Remove(0, 1);
                    string actualMessage = string.Empty;
                    if (msgSplit.Length > 1)
                    {
                        actualMessage = string.Join(" ", msgSplit.Skip(1).Take(msgSplit.Length - 1));
                    }
                    ProcessCommand(cmd, actualMessage, location);
                }
                else
                {
                    _IRC.Command.SendPrivateMessage(location, message);
                }
            }
        }

        private void ProcessCommand(string command, string message, string location)
        {
            switch (command.ToLower())
            {
                case "join":
                    _IRC.Command.SendJoin(message);
                    break;
                case "part":
                    _IRC.Command.SendPart(message);
                    break;
                case "invite":
                    _IRC.Command.SendInvite(location, message);
                    break;
                case "list":
                    _IRC.Command.SendList(message);
                    break;
                case "action":
                    _IRC.Command.SendCTCPMessage(location, "ACTION", message);
                    break;
                case "query":
                    string rec = string.Empty;
                    string msg = string.Empty;
                    string[] querySplit = message.Split(' ');
                    if (querySplit.Length > 0)
                    {
                        rec = querySplit[0];
                    }
                    if (querySplit.Length > 1)
                    {
                        msg = string.Join(" ", querySplit.Skip(1).Take(querySplit.Length - 1));
                    }
                    _IRC.Command.SendPrivateMessage(rec, msg);
                    break;
                case "notice":
                    string notRec = string.Empty;
                    string notMsg = string.Empty;
                    string[] notSplit = message.Split(' ');
                    if (notSplit.Length > 0)
                    {
                        notRec = notSplit[0];
                    }
                    if (notSplit.Length > 1)
                    {
                        notMsg = string.Join(" ", notSplit.Skip(1).Take(notSplit.Length - 1));
                    }
                    _IRC.Command.SendNotice(notRec, notMsg);
                    break;
                case "raw":
                    _IRC.Command.SendRaw(message);
                    break;
                default:
                    _CallerContext.exception($"Command not recognized: {command}");
                    break;

            }
        }

        // Handle Message Events
        private void Message_RawMessageEvent(object sender, string message)
        {
            _CallerContext.rawMessage(message);
        }

        private void Message_ServerReplyEvent(object sender, IReply e)
        {
            if (e.GetType() == typeof(ServerReplyMessage))
            {
                ServerReplyMessage reply = (ServerReplyMessage)e;
                _CallerContext.serverReply(reply);
            }
            else if (e.GetType() == typeof(ServerErrorMessage))
            {
                ServerErrorMessage error = (ServerErrorMessage)e;
                _CallerContext.serverError(error);
            }
        }

        private void Message_NickChangeEvent(object sender, NickChangeInfo e)
        {
            _CallerContext.nickChange(e);
        }

        private void Message_UserModeChangeEvent(object sender, UserModeChangeInfo e)
        {
            _CallerContext.userModeChange(e, e.Modes.ModesToString());
        }

        private void Message_TopicChangeEvent(object sender, TopicChangeInfo e)
        {
            _CallerContext.topicChange(e);
        }

        private void Message_KickEvent(object sender, KickInfo e)
        {
            _CallerContext.kick(e);
        }

        private void Message_QuitEvent(object sender, QuitInfo e)
        {
            _CallerContext.quit(e);
        }

        private void Message_PartChannelEvent(object sender, PartChannelInfo e)
        {
            _CallerContext.partChannel(e);
        }

        private void Message_InviteChannelEvent(object sender, InviteChannelInfo e)
        {
            _CallerContext.inviteChannel(e);
        }

        private void Message_JoinChannelEvent(object sender, JoinChannelInfo e)
        {
            _CallerContext.joinChannel(e);
        }

        private void Message_CTCPNoticeReceivedEvent(object sender, CTCPMessage e)
        {
            _CallerContext.ctcpMessage(e);
        }

        private void Message_CTCPMessageReceivedEvent(object sender, CTCPMessage e)
        {
            _CallerContext.ctcpMessage(e);
        }

        private void Message_PrivateNoticeReceivedEvent(object sender, PrivateNotice e)
        {
            _CallerContext.privateNotice(e);
        }

        private void Message_PrivateMessageReceivedEvent(object sender, PrivateMessage e)
        {
            _CallerContext.privateMessage(e);
        }

        private void Message_ChannelModeChangeEvent(object sender, ChannelModeChangeInfo e)
        {
            _CallerContext.channelModeChange(e, e.Modes.ModesToString());
        }

        private void Message_ChannelNoticeReceivedEvent(object sender, ChannelNotice e)
        {
            _CallerContext.channelNotice(e);
        }

        private void Message_ChannelMessageReceivedEvent(object sender, ChannelMessage e)
        {
            _CallerContext.channelMessage(e);
        }

        // Handle Command Events
        private void Command_PrivateNoticeCommandEvent(object sender, PrivateNoticeCommand e)
        {
            _CallerContext.privateNoticeCommand(e);
        }

        private void Command_PrivateMessageCommandEvent(object sender, PrivateMessageCommand e)
        {
            _CallerContext.privateMessageCommand(e);
        }

        // Handle IRC events
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