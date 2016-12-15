// Copyright (c) .NET Foundation. All rights reserved. Licensed under the Apache License, Version
// 2.0. See License.txt in the project root for license information.

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
                var config = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting this line JSONP requests are insecure but
                    // some older browsers (and some versions of IE) require JSONP to work cross
                    // domain EnableJSONP = true
                    EnableDetailedErrors = true
                };

                // Turns cors support on allowing everything In real applications, the origins should
                // be locked down
                map.UseCors(CorsOptions.AllowAll)
                   .RunSignalR(config);
            });

            log4net.Config.XmlConfigurator.Configure();

            // Turn tracing on programmatically
            //GlobalHost.TraceManager.Switch.Level = SourceLevels.Information;
        }

        #endregion
    }
}