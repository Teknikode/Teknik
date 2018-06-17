using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.TagHelpers
{
    [HtmlTargetElement("version")]
    public class VersionHelper : TagHelper
    {
        private const string _verFile = "version.json";

        public string Source { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Clear the initial wrap tag
            output.TagName = string.Empty;

            // Get the version file info
            string dataDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            string fullPath = Path.Combine(dataDir, _verFile);

            if (File.Exists(fullPath))
            {
                using (StreamReader file = File.OpenText(fullPath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject res = (JObject)JToken.ReadFrom(reader);

                    string commitVer = res["version"].ToString();
                    string commitHash = res["hash"].ToString();

                    output.Content.AppendHtml($"Version: {commitVer} - Hash: <a href=\"{Source}{commitHash}\">{commitHash.Truncate(10)}</a>");
                }
            }
        }
    }
}
