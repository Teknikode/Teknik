using System.Web;
using System.Web.Optimization;
using Teknik.Helpers;

namespace Teknik
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;

            BundleTable.EnableOptimizations = true;
#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif

            bundles.Add(new AzureStyleBundle("~/Content/Common", "https://cdn.teknik.io", "www").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/common.css"));

            bundles.Add(new AzureScriptBundle("~/bundles/common", "https://cdn.teknik.io", "www").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/bootstrap-select.js",
                        "~/Scripts/common.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new AzureScriptBundle("~/bundles/jquery", "https://cdn.teknik.io", "www").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new AzureScriptBundle("~/bundles/modernizr", "https://cdn.teknik.io", "www").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new AzureScriptBundle("~/bundles/markdown", "https://cdn.teknik.io", "www").Include(
                      "~/Scripts/PageDown/Markdown.Converter.js",
                      "~/Scripts/PageDown/Markdown.Sanitizer.js"));
        }
    }
}
