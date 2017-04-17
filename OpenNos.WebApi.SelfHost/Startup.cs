using log4net.Config;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;

namespace OpenNos.WebApi.SelfHost
{
    public class Startup
    {
        #region Methods

        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Map("/signalr", map =>
            {
                HubConfiguration config = new HubConfiguration
                {
                    EnableDetailedErrors = true
                };
                map.UseCors(CorsOptions.AllowAll).RunSignalR(config);
            });
            XmlConfigurator.Configure();
        }

        #endregion
    }
}