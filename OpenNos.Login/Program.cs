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
using System.Collections.Generic;

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
                                     + "                 LOGIN SERVER VERSION 1.0.0 by OpenNos Team\n" +
                                     "===============================================================================\n");

                    string ip = System.Configuration.ConfigurationManager.AppSettings["LoginIp"];
                    int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LoginPort"]);
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("Config_Loaded"));
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
