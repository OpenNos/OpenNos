using log4net;
using OpenNos.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace OpenNos.Login
{
    public class Program
    {
        public static void Main()
        {
            checked
            {
                try
                {
                    LoginPacketHandler loginCore = new LoginPacketHandler();

                    //initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                    Console.Title = "OpenNos_Login ";
                    Console.WriteLine("===============================================================================\n"
                                     + "                 AUTHENTICATION SERVER VERSION 1 by 0Lucifer0\n" +
                                     "==============================================================================\n");

                    if (!File.Exists(String.Format("{0}config.ini", Application.AppPath(true))))
                    {
                        Logger.Log.Error("Config.ini not found!");
                        Console.ReadKey();
                        return;
                    }
                    Logger.Log.Info("Loading Configurations !");

                    Config config = new Config(String.Format("{0}config.ini", Application.AppPath(true)));

                    loginCore.SetData(config.GetString("CONFIGURATION", "Ip", "error"), config.GetString("CONFIGURATION", "Ip_Game", "error"), config.GetInteger("CONFIGURATION", "Login_Port", 5), config.GetString("CONFIGURATION", "Nom_serveur", "error"), config.GetInteger("CONFIGURATION", "Canaux", 5), config.GetInteger("CONFIGURATION", "Game_Port", 5));
                    Logger.Log.Info("Config Loaded !");

                    Dictionary<string, object> packetHandlers = new Dictionary<string, object>();
                    LoginPacketHandler loginHandler = new LoginPacketHandler();
                    packetHandlers.Add(loginHandler.ToString(), loginHandler);

                    NetworkManager<LoginEncryption> networkManager = new NetworkManager<LoginEncryption>(loginCore.GetIp(), loginCore.GetPort(), packetHandlers, false);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                    Console.ReadKey();
                }
            }
        }
    }
}
