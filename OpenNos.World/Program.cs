using log4net;
using OpenNos.Core;
using OpenNos.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.World
{
    public class Program
    {
        static void Main(string[] args)
        {
            //define handers for received packets
            IList<Type> handlers = new List<Type>();
            handlers.Add(typeof(LoginPacketHandler));

            //initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            NetworkManager<WorldEncryption> networkManager = new NetworkManager<WorldEncryption>("127.0.0.1", 1337, handlers, true);
        }
    }
}
