using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using Teknik.Configuration;

namespace Teknik.Helpers
{
    public class CdnScriptBundle : Bundle
    {
        public CdnScriptBundle(string virtualPath, string cdnHost)
            : base(virtualPath, null, new IBundleTransform[] { new JsMinify(), new CdnBundleTransform(cdnHost, ".js") })
        {
            ConcatenationToken = ";";
        }
    }

    public class CdnStyleBundle : Bundle
    {
        public CdnStyleBundle(string virtualPath, string cdnHost)
            : base(virtualPath, null, new IBundleTransform[] { new CssMinify(), new CdnBundleTransform(cdnHost, ".css") })
        {
        }
    }

    public class CdnBundleTransform : IBundleTransform
    {
        public string CdnHost { get; set; }

        public string Ext { get; set; }

        public CdnBundleTransform(string cdnHost, string ext)
        {
            CdnHost = cdnHost;
            Ext = ext;
        }

        public virtual void Process(BundleContext context, BundleResponse response)
        {           
            // Don't continue if we aren't using a CDN
            if (!context.BundleCollection.UseCdn)
            {
                return;
            }

            // Get the directory and filename for the bundle
            var dir = VirtualPathUtility.GetDirectory(context.BundleVirtualPath).TrimStart('/').TrimStart('~').TrimStart('/').TrimEnd('/');
            var file = VirtualPathUtility.GetFileName(context.BundleVirtualPath);
            var group = string.Format("{0}{1}", file, Ext);

            if (string.IsNullOrEmpty(CdnHost))
            {
                return;
            }            

            using (var hashAlgorithm = SHA256.CreateHashAlgorithm())
            {
                var hash = HttpServerUtility.UrlTokenEncode(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(response.Content)));
                context.BundleCollection.GetBundleFor(context.BundleVirtualPath).CdnPath = string.Format("{0}/{1}/{2}?v={3}&group={4}", CdnHost.TrimEnd('/'), dir, file, hash, group);
            }
        }
    }
}
