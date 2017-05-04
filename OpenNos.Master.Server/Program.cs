using log4net;

using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.ScsServices.Service;
using OpenNos.DAL;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data;
using OpenNos.Master.Interface;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OpenNos.Master.Server
{

    class Program
    {
        private static ManualResetEvent run = new ManualResetEvent(true);
        private static IScsServiceApplication _server;
        private static CommunicationService _communicationService;

        static void Main(string[] args)
        {
            try
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                Console.Title = $"OpenNos Master Server v{fileVersionInfo.ProductVersion}dev";
                string ipAddress = ConfigurationManager.AppSettings["MasterIPAddress"];
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
                string text = $"MASTER SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                int offset = Console.WindowWidth / 2 + text.Length / 2;
                string separator = new string('=', Console.WindowWidth);
                Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);

                // initialize DB
                if (!DataAccessHelper.Initialize())
                {
                    Console.ReadLine();
                    return;
                }

                Logger.Log.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

                try
                {
                    // register EF -> GO and GO -> EF mappings
                    RegisterMappings();

                    // configure Services and Service Host
                    _server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));
                    _communicationService = new CommunicationService();

                    _server.AddService<ICommunicationService, CommunicationService>(_communicationService);

                    _server.Start();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("General Error Server", ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
                Console.ReadKey();
            }
        }

        private static void RegisterMappings()
        {
            DAOFactory.AccountDAO.RegisterMapping(typeof(AccountDTO)).InitializeMapper();
            DAOFactory.CharacterDAO.RegisterMapping(typeof(CharacterDTO)).InitializeMapper();
            DAOFactory.PenaltyLogDAO.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
        }
    }
}
