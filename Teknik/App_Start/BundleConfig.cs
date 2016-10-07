using System.Web;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Helpers;

namespace Teknik
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Load the config options
            Config config = Config.Load();

            // Set if we are using Cdn
            bundles.UseCdn = config.UseCdn;

            BundleTable.EnableOptimizations = false;
#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif

            bundles.Add(new CdnStyleBundle("~/Content/Common", config.CdnHost).Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/common.css"));

            bundles.Add(new CdnScriptBundle("~/bundles/common", config.CdnHost).Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/bootstrap-select.js",
                        "~/Scripts/common.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new CdnScriptBundle("~/bundles/jquery", config.CdnHost).Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new CdnScriptBundle("~/bundles/modernizr", config.CdnHost).Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new CdnScriptBundle("~/bundles/markdown", config.CdnHost).Include(
                      "~/Scripts/PageDown/Markdown.Converter.js",
                      "~/Scripts/PageDown/Markdown.Sanitizer.js"));
        }
    }
}
