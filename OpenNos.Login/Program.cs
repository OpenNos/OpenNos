using log4net;
using OpenNos.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace OpenNos.Login
{
    class Program
    {
        private static int session = 0;

        public static void Main()
        {
            checked
            {
                try
                {
                    Login loginCore = new Login();

                    //initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                    Console.Title = "OpenNos_Login ";
                    Console.WriteLine("===============================================================================\n"
                                     + "                 AUTHENTICATION SERVER VERSION 1 by 0Lucifer0\n" +
                                     "==============================================================================\n");

                    if (!File.Exists(String.Format("{0}config.ini",Program.AppPath(true))))
                    {
                        Logger.Log.Error("Config.ini not found!");
                        Console.ReadKey();
                        return;
                    }
                    Logger.Log.Info("Loading Configurations !");
                    
                    Config config = new Config(String.Format("{0}config.ini", Program.AppPath(true)));

                    loginCore.SetData(config.GetString("CONFIGURATION", "Ip", "error"), config.GetString("CONFIGURATION", "Ip_Game", "error"), config.GetInteger("CONFIGURATION", "Login_Port", 5), config.GetString("CONFIGURATION", "Nom_serveur", "error"), config.GetInteger("CONFIGURATION", "Canaux", 5), config.GetInteger("CONFIGURATION", "Game_Port", 5));
                    Logger.Log.Info("Config Loaded !");

                    TcpListener tcpListener = new TcpListener(IPAddress.Parse(loginCore.GetIp()), loginCore.GetPort());
                    tcpListener.Start();

                    Logger.Log.Info("Server ON !");

                    do
                    {
                        TcpClient tcpClient = tcpListener.AcceptTcpClient();
                        byte[] array = new byte[tcpClient.ReceiveBufferSize + 1];
                        NetworkStream stream = tcpClient.GetStream();
                        stream.Read(array, 0, tcpClient.ReceiveBufferSize);
                        loginCore.CheckUser(loginCore.GetUser(Encryption.LoginDecrypt(array, array.Length)), stream, Program.session);
                        tcpClient.Close();
                        Program.session += 2;
                    }
                    while (true);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                    Console.ReadKey();
                }
            }
        }
        public static string AppPath(bool backSlash = true)
        {
            string text = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            if (backSlash)
            {
                text += "\\";
            }
            return text;
        }

    }
}
