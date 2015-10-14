using Microsoft.VisualBasic.CompilerServices;
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

                    bool flag = !File.Exists(MainFile.AppPath(true) + "config/LoginServer.ini");
                    if (flag)
                    {
                        ConsoleTools.WriteConsole("ERROR", "Config.ini not found!");
                        Console.ReadKey();
                        ProjectData.EndApp();
                    }
                    ConsoleTools.WriteConsole("INFO", "Loading Configurations !");
                    ConfigIni ConfIni = new ConfigIni(MainFile.AppPath(true) + "config/LoginServer.ini");

                    loginCore.SetData(ConfIni.GetString("CONFIGURATION", "Ip", "error"), ConfIni.GetString("CONFIGURATION", "Ip_Game", "error"), ConfIni.GetInteger("CONFIGURATION", "Login_Port", 5), ConfIni.GetString("CONFIGURATION", "Nom_serveur", "error"), ConfIni.GetInteger("CONFIGURATION", "Canaux", 5), ConfIni.GetInteger("CONFIGURATION", "Game_Port", 5));

                    Mysql mysql = new Mysql();
                    mysql.SetData(ConfIni.GetString("MYSQL", "Ip", "error"), ConfIni.GetString("MYSQL", "User", "error"), ConfIni.GetString("MYSQL", "Password", "error"), ConfIni.GetString("MYSQL", "Database", "error"));
                    bool test = mysql.StartMysql();
                    if (test == false)
                    {
                        ConsoleTools.WriteConsole("ERROR", "Bad MySql configuration !");
                        Console.ReadKey();
                        ProjectData.EndApp();
                    }
                    else
                    {
                        ConsoleTools.WriteConsole("INFO", "Loading Config !");

                        TcpListener tcpListener = new TcpListener(IPAddress.Parse(loginCore.GetIp()), loginCore.GetPort());
                        tcpListener.Start();
                        ConsoleTools.WriteConsole("INFO", "Serveur ON !");


                        do
                        {
                            TcpClient tcpClient = tcpListener.AcceptTcpClient();
                            byte[] array = new byte[tcpClient.ReceiveBufferSize + 1];
                            NetworkStream stream = tcpClient.GetStream();
                            stream.Read(array, 0, tcpClient.ReceiveBufferSize);
                            loginCore.CheckUser(loginCore.GetUser(Encryption.LoginDecrypt(array, array.Length)), mysql, stream, MainFile.session);
                            tcpClient.Close();
                            MainFile.session += 2;
                        }
                        while (true);
                    }
                }
                catch (Exception ex)
                {
                    ProjectData.SetProjectError(ex);
                    ConsoleTools.WriteConsole("ERROR", ex.Message);
                    Console.ReadKey();
                    ProjectData.ClearProjectError();
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
