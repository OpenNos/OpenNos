using log4net;
using Microsoft.Owin.Hosting;
using OpenNos.Core;
using System;
using System.Diagnostics;
using System.Reflection;

namespace OpenNos.WebApi.SelfHost
{
    public class Program
    {
        #region Methods

        private static void Main()
        {
            using (WebApp.Start<Startup>("http://localhost:6666"))
            {
                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(ServerCommunicationHub)));
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Console.Title = $"OpenNos Server Communication {fileVersionInfo.ProductVersion}dev";
                const string text = "SERVER COMMUNICATION - PORT: 6666 by OpenNos Team";
                int offset = Console.WindowWidth / 2 + text.Length / 2;
                string separator = new string('=', Console.WindowWidth);
                Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
                Console.ReadKey();
            }
        }

        #endregion
    }
}