﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Teknik.Utilities
{
    public static class HttpRequestExtensions
    {
        public static string GetClientIpAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return IPAddress.Parse(((HttpContext)request.Properties["MS_HttpContext"]).Request.Host.Value).ToString();
            }
            return null;
        }
    }
}
