/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */
using log4net;
using OpenNos.Core;
using System;
using System.Linq;
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
                    //define handers for received packets
                    IList<Type> handlers = new List<Type>();
                    handlers.Add(typeof(LoginPacketHandler));

                    //initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                    Console.Title = "OpenNos Login Server v1.0.0";
                    Console.WriteLine("===============================================================================\n"
                                     + "                 LOGIN SERVER VERSION 1.0.0 by OpenNos Team0\n" +
                                     "==============================================================================\n");

                    if (!File.Exists(String.Format("{0}config.ini", Application.AppPath(true))))
                    {
                        Logger.Log.Error("Config.ini not found!");
                        Console.ReadKey();
                        return;
                    }
                    Logger.Log.Info("Loading Configurations !");

                    Config config = new Config(String.Format("{0}config.ini", Application.AppPath(true)));

                    //take this logic directly to LoginPacketHandler, maybe make config static and accessible from everywhere
                    //loginCore.SetData(config.GetString("CONFIGURATION", "Ip", "error"), config.GetString("CONFIGURATION", "Ip_Game", "error"), config.GetInteger("CONFIGURATION", "Login_Port", 5), config.GetString("CONFIGURATION", "Nom_serveur", "error"), config.GetInteger("CONFIGURATION", "Canaux", 5), config.GetInteger("CONFIGURATION", "Game_Port", 5));
                    Logger.Log.Info("Config Loaded !");

                    string ip = config.GetString("CONFIGURATION","Ip");
                    int port = config.GetInteger("CONFIGURATION", "Login_Port");
                    NetworkManager<LoginEncryption> networkManager = new NetworkManager<LoginEncryption>(ip,port, handlers, false);
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
