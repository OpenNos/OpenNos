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
using OpenNos.Handler;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenNos.World
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //define handers for received packets
            IList<Type> handlers = new List<Type>();
            handlers.Add(typeof(AccountPacketHandler));

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
