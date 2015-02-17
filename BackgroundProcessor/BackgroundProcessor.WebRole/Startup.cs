using BackgroundProcessor.WebRole;

using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace BackgroundProcessor.WebRole
{
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
