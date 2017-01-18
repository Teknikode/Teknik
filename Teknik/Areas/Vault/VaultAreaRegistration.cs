using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Vault
{
    public class VaultAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Vault";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Vault.Index",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Vault", action = "Index" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/vault", config.CdnHost).Include(
                      "~/Areas/Vault/Scripts/Vault.js"));
        }
    }
}