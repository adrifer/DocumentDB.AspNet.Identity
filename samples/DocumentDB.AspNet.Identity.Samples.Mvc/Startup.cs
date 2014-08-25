using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DocumentDB.AspNet.Identity.Samples.Mvc.Startup))]
namespace DocumentDB.AspNet.Identity.Samples.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
