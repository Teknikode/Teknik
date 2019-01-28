using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.IdentityServer.Security;
using Teknik.IdentityServer.ViewModels;
using Teknik.Logging;

namespace Teknik.IdentityServer.Controllers
{
    public class HomeController : DefaultController
    {
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(
            ILogger<Logger> logger,
            Config config, 
            IIdentityServerInteractionService interaction) : base(logger, config)
        {
            _interaction = interaction;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}