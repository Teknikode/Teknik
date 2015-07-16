using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Teknik.Startup))]
namespace Teknik
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
