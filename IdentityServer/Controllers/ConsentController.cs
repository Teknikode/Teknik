using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.IdentityServer.Models;
using Teknik.IdentityServer.Security;
using Teknik.IdentityServer.Services;
using Teknik.Logging;

namespace Teknik.IdentityServer.Controllers
{
    /// <summary>
    /// This controller processes the consent UI
    /// </summary>
    public class ConsentController : DefaultController
    {
        private readonly ConsentService _consent;

        public ConsentController(
            ILogger<Logger> logger,
            Config config,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IResourceStore resourceStore) : base(logger, config)
        {
            _consent = new ConsentService(interaction, clientStore, resourceStore, logger);
        }

        /// <summary>
        /// Shows the consent screen
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = await _consent.BuildViewModelAsync(returnUrl);
            if (vm != null)
            {
                return View("Index", vm);
            }

            throw new ApplicationException($"Unable to load consent view model.");
        }

        /// <summary>
        /// Handles the consent screen postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConsentInputModel model)
        {
            var result = await _consent.ProcessConsent(model);

            if (result.IsRedirect)
            {
                return Redirect(result.RedirectUri);
            }

            if (result.HasValidationError)
            {
                ModelState.AddModelError("", result.ValidationError);
            }

            if (result.ShowView)
            {
                return View("Index", result.ViewModel);
            }

            throw new ApplicationException($"Unable to load consent view model.");
        }
    }
}