using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Loupe.Agent.Web.Module.MVCTest.Startup))]
namespace Loupe.Agent.Web.Module.MVCTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
