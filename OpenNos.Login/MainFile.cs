using log4net;
using OpenNos.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace OpenNos.Login
{
    class MainFile
    {
        private static int session = 0;
        private static readonly ILog log = LogManager.GetLogger(typeof(MainFile));

        public static void Main()
        {
            checked
            {
                try
                {
                    Login loginCore = new Login();


                    Console.Title = "OpenNos_Login ";
                    Console.WriteLine("===============================================================================\n"
                                     + "                 AUTHENTICATION SERVER VERSION 1 by 0Lucifer0\n" +
                                     "==============================================================================\n");

                    bool flag = !File.Exists(MainFile.AppPath(true) + "config.ini");
                    if (flag)
                    {
                        log.Error("Config.ini not found!");
                        Console.ReadKey();
                        return;
                    }
                    log.Info("Loading Configurations !");
                    
                    Config ConfIni = new Config(MainFile.AppPath(true) + "config.ini");

                    loginCore.SetData(ConfIni.GetString("CONFIGURATION", "Ip", "error"), ConfIni.GetString("CONFIGURATION", "Ip_Game", "error"), ConfIni.GetInteger("CONFIGURATION", "Login_Port", 5), ConfIni.GetString("CONFIGURATION", "Nom_serveur", "error"), ConfIni.GetInteger("CONFIGURATION", "Canaux", 5), ConfIni.GetInteger("CONFIGURATION", "Game_Port", 5));
                    log.Info("Config Loaded !");

                    TcpListener tcpListener = new TcpListener(IPAddress.Parse(loginCore.GetIp()), loginCore.GetPort());
                    tcpListener.Start();
                  
                    log.Info("Server ON !");

                    do
                    {
                        TcpClient tcpClient = tcpListener.AcceptTcpClient();
                        byte[] array = new byte[tcpClient.ReceiveBufferSize + 1];
                        NetworkStream stream = tcpClient.GetStream();
                        stream.Read(array, 0, tcpClient.ReceiveBufferSize);
                        loginCore.CheckUser(loginCore.GetUser(Encryption.LoginDecrypt(array, array.Length)), stream, MainFile.session);
                        tcpClient.Close();
                        MainFile.session += 2;
                    }
                    while (true);
                }
                catch (Exception ex)
                {
                    ConsoleTools.WriteConsole("ERROR", ex.Message);
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
