using Media.Rtsp;
using Media.Rtsp.Server.MediaTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Models;

namespace TeknikStreaming
{
    public partial class Server : ServiceBase
    {
        private RtspServer _RTSPServer;

        public Server()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _RTSPServer = new RtspServer(System.Net.Sockets.AddressFamily.NetBios, 555);

            LoadStreams();

            _RTSPServer.Start();
        }

        protected override void OnStop()
        {
            _RTSPServer.Stop();
        }

        private void LoadStreams()
        {
            TeknikEntities db = new TeknikEntities();

            List<User> users = db.Users.ToList();
            if (users != null)
            {
                foreach (User user in users)
                {
                    RtspSource source = new RtspSource(string.Format("TeknikLiveStream_{0}", user.Username), string.Format("rtsp://localhost/live/{0}/stream.amp", user.Username));

                }
            }
        }
    }
}
