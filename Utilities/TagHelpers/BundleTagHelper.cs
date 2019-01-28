using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teknik.Utilities.TagHelpers
{
    [HtmlTargetElement("bundle")]
    public class BundleTagHelper : TagHelper
    {
        private const string VirtualFolder = "./wwwroot/";
        private const string ConfigPath = "bundleconfig.json";

        private readonly IHostingEnvironment _env;

        private readonly IMemoryCache _cache;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public bool AppendVersion { get; set; } = false;

        public string Src { get; set; }

        public BundleTagHelper(IHostingEnvironment env, IMemoryCache cache)
        {
            _env = env;
            _cache = cache;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Clear the initial wrap tag
            output.TagName = string.Empty;

            // Clean up the src
            Src = Src.TrimStart(new [] { '~', '/' });

            List<string> files = new List<string>();

            switch (_env.EnvironmentName)
            {
                case "Test":
                case "Production":
                    files.Add(Src);
                    break;
                case "Development":
                    var configFile = Path.Combine(_env.ContentRootPath, ConfigPath);
                    var bundle = GetBundle(configFile, Src);
                    if (bundle == null)
                        return;

                    // Clean up the bundle to remove the virtual folder that aspnetcore provides.
                    files.AddRange(bundle.InputFiles.Select(file => 
                    {
                        if (file.StartsWith(VirtualFolder))
                            return file.Substring(VirtualFolder.Length);
                        return file;
                    }).ToList());
                    break;
            }

            foreach (var file in files)
            {
                // Get the file version for this file
                string fullPath = "/" + file;

                if (AppendVersion)
                {
                    var versionProvider = new FileVersionProvider(_env.WebRootFileProvider, _cache, ViewContext.HttpContext.Request.Path);
                    fullPath = versionProvider.AddFileVersionToPath(fullPath);
                }

                if (file.EndsWith(".js"))
                {
                    output.Content.AppendHtml($"<script src='{fullPath}' type='text/javascript'></script>");
                }
                else if (file.EndsWith(".css"))
                {
                    output.Content.AppendHtml($"<link rel='stylesheet' href='{fullPath}'></link>");
                }
            }
        }

        public static Bundle GetBundle(string configFile, string bundlePath)
        {
            var file = new FileInfo(configFile);
            if (!file.Exists)
                return null;

            var bundles = JsonConvert.DeserializeObject<IEnumerable<Bundle>>(File.ReadAllText(configFile));
            return (from b in bundles
                    where b.OutputFileName.EndsWith(bundlePath, StringComparison.InvariantCultureIgnoreCase)
                    select b).FirstOrDefault();
        }

        public class Bundle
        {
            [JsonProperty("outputFileName")]
            public string OutputFileName { get; set; }

            [JsonProperty("inputFiles")]
            public List<string> InputFiles { get; set; } = new List<string>();
        }
    }
}
