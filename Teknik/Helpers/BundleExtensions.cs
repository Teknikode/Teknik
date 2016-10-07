using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;

namespace Teknik.Helpers
{
    public class AzureScriptBundle : Bundle
    {
        public AzureScriptBundle(string virtualPath, string cdnHost, string subdomain)
            : base(virtualPath, null, new IBundleTransform[] { new JsMinify(), new AzureBundleTransform { CdnHost = cdnHost, Subdomain = subdomain } })
        {
            ConcatenationToken = ";";
        }
    }

    public class AzureStyleBundle : Bundle
    {
        public AzureStyleBundle(string virtualPath, string cdnHost, string subdomain)
            : base(virtualPath, null, new IBundleTransform[] { new CssMinify(), new AzureBundleTransform { CdnHost = cdnHost, Subdomain = subdomain } })
        {
        }
    }

    public class AzureBundleTransform : IBundleTransform
    {
        public string CdnHost { get; set; }

        public string Subdomain { get; set; }

        static AzureBundleTransform()
        {
        }

        public virtual void Process(BundleContext context, BundleResponse response)
        {
            var dir = VirtualPathUtility.GetDirectory(context.BundleVirtualPath).TrimStart('/').TrimStart('~').TrimStart('/').TrimEnd('/');
            var file = VirtualPathUtility.GetFileName(context.BundleVirtualPath);

            if (!context.BundleCollection.UseCdn)
            {
                return;
            }

            if (string.IsNullOrEmpty(CdnHost) || string.IsNullOrEmpty(Subdomain))
            {
                return;
            }            

            using (var hashAlgorithm = SHA256.CreateHashAlgorithm())
            {
                var hash = HttpServerUtility.UrlTokenEncode(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(response.Content)));
                context.BundleCollection.GetBundleFor(context.BundleVirtualPath).CdnPath = string.Format("{0}/{1}/{2}?sub={3}&v={4}", CdnHost.TrimEnd('/'), dir, file, Subdomain, hash);
            }
        }
    }
}
