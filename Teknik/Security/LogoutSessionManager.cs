using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Security
{
    public class LogoutSessionManager
    {
        private static List<Session> _sessions = new List<Session>();

        public void Add(string sub, string sid)
        {
            _sessions.Add(new Session { Sub = sub, Sid = sid });
        }

        public bool IsLoggedOut(string sub, string sid)
        {
            var matches = _sessions.Any(s => s.IsMatch(sub, sid));
            return matches;
        }

        private class Session
        {
            public string Sub { get; set; }
            public string Sid { get; set; }

            public bool IsMatch(string sub, string sid)
            {
                return (Sid == sid && Sub == sub) ||
                       (Sid == sid && Sub == null) ||
                       (Sid == null && Sub == sub);
            }
        }
    }
}
