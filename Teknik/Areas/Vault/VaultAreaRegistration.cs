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
                 "Vault.Create",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Vault", action = "Create" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            context.MapSubdomainRoute(
                 "Vault.ViewVault",
                 new List<string>() { "vault", "v" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{id}",
                 new { controller = "Vault", action = "ViewVault" },
                 new[] { typeof(Controllers.VaultController).Namespace }
             );

            // Register style bundles
            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/vault", config.CdnHost).Include(
                      "~/Areas/Vault/Content/Vault.css"));

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/vault", config.CdnHost).Include(
                      "~/Areas/Vault/Scripts/Vault.js"));
        }
    }
}