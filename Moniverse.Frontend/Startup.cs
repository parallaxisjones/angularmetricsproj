using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PlayverseMetrics.Startup))]
namespace PlayverseMetrics
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
