using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ValiModern.Startup))]

namespace ValiModern
{
    /// <summary>
    /// OWIN Startup for SignalR
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable SignalR
            app.MapSignalR();
        }
    }
}
