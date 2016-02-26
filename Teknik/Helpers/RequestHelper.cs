using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Teknik.Helpers
{
    public static class RequestHelper
    {
        /// <summary>
        /// method to get Client ip address
        /// </summary>
        /// <param name="GetLan"> set to true if want to get local(LAN) Connected ip address</param>
        /// <returns></returns>
        public static string GetVisitorIPAddress(bool GetLan = false)
        {
            string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                GetLan = true;
                visitorIPAddress = string.Empty;
            }

            if (GetLan)
            {
                if (string.IsNullOrEmpty(visitorIPAddress))
                {
                    //This is for Local(LAN) Connected ID Address
                    string stringHostName = Dns.GetHostName();
                    //Get Ip Host Entry
                    IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                    //Get Ip Address From The Ip Host Entry Address List
                    IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                    try
                    {
                        visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                    }
                    catch
                    {
                        try
                        {
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            try
                            {
                                arrIpAddress = Dns.GetHostAddresses(stringHostName);
                                visitorIPAddress = arrIpAddress[0].ToString();
                            }
                            catch
                            {
                                visitorIPAddress = "127.0.0.1";
                            }
                        }
                    }
                }
            }
            return visitorIPAddress;
        }

        // based on http://www.grantburton.com/2008/11/30/fix-for-incorrect-ip-addresses-in-wordpress-comments/
        public static string ClientIPFromRequest(this HttpRequestBase request, bool skipPrivate)
        {
            foreach (var item in s_HeaderItems)
            {
                var ipString = request.Headers[item.Key];

                if (String.IsNullOrEmpty(ipString))
                    continue;

                if (item.Split)
                {
                    foreach (var ip in ipString.Split(','))
                        if (ValidIP(ip, skipPrivate))
                            return ip;
                }
                else
                {
                    if (ValidIP(ipString, skipPrivate))
                        return ipString;
                }
            }

            return request.UserHostAddress;
        }

        public static string DumpHeaders(this HttpRequestBase request)
        {
            var headers = string.Empty;
            foreach (var key in request.Headers.AllKeys)
                headers += key + "=" + request.Headers[key] + Environment.NewLine;
            return headers;
        }

        public static string DumpServerVariables(this HttpRequestBase request)
        {
            var variables = string.Empty;
            foreach (var key in request.ServerVariables.AllKeys)
                variables += key + "=" + request.ServerVariables[key] + Environment.NewLine;
            return variables;
        }

        private static bool ValidIP(string ip, bool skipPrivate)
        {
            IPAddress ipAddr;

            ip = ip == null ? String.Empty : ip.Trim();

            if (0 == ip.Length
                || false == IPAddress.TryParse(ip, out ipAddr)
                || (ipAddr.AddressFamily != AddressFamily.InterNetwork
                    && ipAddr.AddressFamily != AddressFamily.InterNetworkV6))
                return false;

            if (skipPrivate && ipAddr.AddressFamily == AddressFamily.InterNetwork)
            {
                var addr = IpRange.AddrToUInt64(ipAddr);
                foreach (var range in s_PrivateRanges)
                {
                    if (range.Encompasses(addr))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Provides a simple class that understands how to parse and
        /// compare IP addresses (IPV4) ranges.
        /// </summary>
        private sealed class IpRange
        {
            private readonly UInt64 _start;
            private readonly UInt64 _end;

            public IpRange(string startStr, string endStr)
            {
                _start = ParseToUInt64(startStr);
                _end = ParseToUInt64(endStr);
            }

            public static UInt64 AddrToUInt64(IPAddress ip)
            {
                var ipBytes = ip.GetAddressBytes();
                UInt64 value = 0;

                foreach (var abyte in ipBytes)
                {
                    value <<= 8;    // shift
                    value += abyte;
                }

                return value;
            }

            public static UInt64 ParseToUInt64(string ipStr)
            {
                var ip = IPAddress.Parse(ipStr);
                return AddrToUInt64(ip);
            }

            public bool Encompasses(UInt64 addrValue)
            {
                return _start <= addrValue && addrValue <= _end;
            }

            public bool Encompasses(IPAddress addr)
            {
                var value = AddrToUInt64(addr);
                return Encompasses(value);
            }
        };

        private static readonly IpRange[] s_PrivateRanges =
            new IpRange[] {
            new IpRange("0.0.0.0","2.255.255.255"),
            new IpRange("10.0.0.0","10.255.255.255"),
            new IpRange("127.0.0.0","127.255.255.255"),
            new IpRange("169.254.0.0","169.254.255.255"),
            new IpRange("172.16.0.0","172.31.255.255"),
            new IpRange("192.0.2.0","192.0.2.255"),
            new IpRange("192.168.0.0","192.168.255.255"),
            new IpRange("255.255.255.0","255.255.255.255")
            };


        /// <summary>
        /// Describes a header item (key) and if it is expected to be 
        /// a comma-delimited string
        /// </summary>
        private sealed class HeaderItem
        {
            public readonly string Key;
            public readonly bool Split;

            public HeaderItem(string key, bool split)
            {
                Key = key;
                Split = split;
            }
        }

        // order is in trust/use order top to bottom
        private static readonly HeaderItem[] s_HeaderItems =
            new HeaderItem[] {
            new HeaderItem("HTTP_CLIENT_IP",false),
            new HeaderItem("HTTP_X_FORWARDED_FOR",true),
            new HeaderItem("HTTP_X_FORWARDED",false),
            new HeaderItem("HTTP_X_CLUSTER_CLIENT_IP",false),
            new HeaderItem("HTTP_FORWARDED_FOR",false),
            new HeaderItem("HTTP_FORWARDED",false),
            new HeaderItem("HTTP_VIA",false),
            new HeaderItem("REMOTE_ADDR",false)
            };

    }
}
