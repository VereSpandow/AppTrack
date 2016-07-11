using Microsoft.Owin;
using Owin;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(AppTrack.Startup))]
namespace AppTrack
{
    public partial class Startup
    {        
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
