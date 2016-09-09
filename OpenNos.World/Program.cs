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
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.GameObject;
using OpenNos.Handler;
using OpenNos.ServiceRef.Internal;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.World
{
    public class Program
    {
        #region Members

        private static EventHandler exitHandler;
        private static ManualResetEvent run = new ManualResetEvent(true);

        #endregion

        #region Delegates

        private delegate bool EventHandler(CtrlType sig);

        #endregion

        #region Enums

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
            //initialize Logger
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.Title = $"OpenNos World Server v{fileVersionInfo.ProductVersion}";
            int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["WorldPort"]);
            string text = $"WORLD SERVER VERSION {fileVersionInfo.ProductVersion} - PORT : {port} by OpenNos Team";
            int offset = (Console.WindowWidth - text.Length) / 2;
            Console.WriteLine("===============================================================================");
            Console.SetCursorPosition(offset < 0 ? 0 : offset, Console.CursorTop);
            Console.WriteLine(text + "\n" +
            "===============================================================================\n");
            //initialize DB
            if (DataAccessHelper.Initialize())
                //initialilize maps
                ServerManager.Initialize();

            //register mappings for items
            DAOFactory.InventoryDAO.RegisterMapping(typeof(SpecialistInstance));
            DAOFactory.InventoryDAO.RegisterMapping(typeof(WearableInstance));
            DAOFactory.InventoryDAO.RegisterMapping(typeof(UsableInstance));
            DAOFactory.InventoryDAO.InitializeMapper(typeof(ItemInstance));

            //initialize ClientLinkManager
            //TODO

            try
            {
                ServiceFactory.Instance.CommunicationService.Open();
                exitHandler += new EventHandler(ExitHandler);
                SetConsoleCtrlHandler(exitHandler, true);
                NetworkManager<WorldEncryption> networkManager = new NetworkManager<WorldEncryption>("127.0.0.1", port, typeof(CommandPacketHandler), typeof(LoginEncryption), true);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        private static bool ExitHandler(CtrlType sig)
        {
            ServerManager.Instance.SaveAll();
            return false;
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        #endregion
    }
}