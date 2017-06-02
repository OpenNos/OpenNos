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
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.Handler;
using OpenNos.Master.Library;
using OpenNos.Master.Library.Client;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace OpenNos.Login
{
    public class Program
    {
        #region Methods

        public static void Main()
        {
            checked
            {
                try
                {
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
#if DEBUG
                    Thread.Sleep(1000);
#endif
                    // initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                    Console.Title = $"OpenNos Login Server v{fileVersionInfo.ProductVersion}dev";
                    int port = Convert.ToInt32(ConfigurationManager.AppSettings["LoginPort"]);
                    string text = $"LOGIN SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                    int offset = Console.WindowWidth / 2 + text.Length / 2;
                    string separator = new string('=', Console.WindowWidth);
                    Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);

                    // initialize api
                    if (CommunicationServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["MasterAuthKey"]))
                    {
                        Logger.Log.Info(Language.Instance.GetMessageFromKey("API_INITIALIZED"));
                    }

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

                        // initialize PacketSerialization
                        PacketFactory.Initialize<WalkPacket>();

                        NetworkManager<LoginEncryption> networkManager = new NetworkManager<LoginEncryption>("127.0.0.1", port, typeof(LoginPacketHandler), typeof(LoginEncryption), false);

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
        }

        private static void RegisterMappings()
        {
            // entities
            DAOFactory.AccountDAO.RegisterMapping(typeof(Account)).InitializeMapper();
            DAOFactory.PenaltyLogDAO.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
        }

        #endregion
    }
}