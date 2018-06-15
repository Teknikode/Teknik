using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Sockets;

namespace Teknik.Utilities
{
    public static class RequestHelper
    {
        // based on http://www.grantburton.com/2008/11/30/fix-for-incorrect-ip-addresses-in-wordpress-comments/
        public static string ClientIPFromRequest(this HttpRequest request, bool skipPrivate)
        {
            foreach (var item in s_HeaderItems)
            {
                var ipString = request.Headers[item.Key].ToString();

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

            return request.Host.Value;
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
