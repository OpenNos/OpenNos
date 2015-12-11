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
using OpenNos.DAL;
using OpenNos.DAL.EF.MySQL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.Handler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace OpenNos.World
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //define handers for received packets
            IList<Type> handlers = new List<Type>();
            handlers.Add(typeof(WorldPacketHandler));

            //initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.Title = String.Format("OpenNos World Server v{0}", fileVersionInfo.ProductVersion);
            Console.WriteLine(String.Format("===============================================================================\n"
                             + "                 WORLD SERVER VERSION {0} by OpenNos Team\n" +
                             "===============================================================================\n", fileVersionInfo.ProductVersion));
          
            //initialize DB
            DataAccessHelper.Initialize();
            //initialilize maps
            ServerManager.Initialize();
            string ip = System.Configuration.ConfigurationManager.AppSettings["WorldIp"];
            int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["WorldPort"]);
            NetworkManager<WorldEncryption> networkManager = new NetworkManager<WorldEncryption>(ip, port, handlers);
            
        }
    }
}
