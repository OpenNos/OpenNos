using log4net;
using OpenNos.Core;
using OpenNos.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OpenNos.World
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //define handers for received packets
            IList<Type> handlers = new List<Type>();
            handlers.Add(typeof(LoginPacketHandler));

            //initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

            Console.Title = "OpenNos World Server v1.0.0";
            Console.WriteLine("===============================================================================\n"
                             + "                 WORLD SERVER VERSION 1.0.0 by OpenNos Team\n" +
                             "===============================================================================\n");

            NetworkManager<WorldEncryption> networkManager = new NetworkManager<WorldEncryption>("127.0.0.1", 1337, handlers, true);
        }
    }
}
