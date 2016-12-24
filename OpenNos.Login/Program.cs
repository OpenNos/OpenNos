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
using OpenNos.WebApi.Reference;
using System;
using System.Diagnostics;
using System.Reflection;

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
                    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

                    // initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                    Console.Title = $"OpenNos Login Server v{fileVersionInfo.ProductVersion}";
                    int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LoginPort"]);
                    string text = $"LOGIN SERVER v{fileVersionInfo.ProductVersion} - PORT : {port} by OpenNos Team";
                    int offset = Console.WindowWidth / 2 + text.Length / 2;
                    string separator = new string('=', Console.WindowWidth);
                    Console.WriteLine(separator + string.Format("{0," + offset + "}", text) + "\n" + separator);

                    // initialize api
                    ServerCommunicationClient.Instance.InitializeAndRegisterCallbacks();

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

                        NetworkManager<LoginEncryption> networkManager = new NetworkManager<LoginEncryption>("127.0.0.1", port, typeof(LoginPacketHandler), typeof(LoginEncryption), false);

                        //cleanup api
                        ServerCommunicationClient.Instance.HubProxy.Invoke("Cleanup");
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