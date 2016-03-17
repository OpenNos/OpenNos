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
using OpenNos.DAL.EF.MySQL;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace OpenNos.Import.Console
{
    public class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            System.Console.Title = $"OpenNos Import Console v{fileVersionInfo.ProductVersion}";
            System.Console.WriteLine("===============================================================================\n"
                             + $"                 IMPORT CONSOLE VERSION {fileVersionInfo.ProductVersion} by OpenNos Team\n" +
                             "===============================================================================\n");

            DataAccessHelper.Initialize();

            Logger.Log.Warn(Language.Instance.GetMessageFromKey("NEED_TREE"));
            System.Console.BackgroundColor = System.ConsoleColor.Blue;
            System.Console.WriteLine("Root");
            System.Console.ResetColor();
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_Item.txt");
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt");
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_monster.txt");
            System.Console.WriteLine($"-----Item.dat");
            System.Console.WriteLine($"-----MapIDData.dat");
            System.Console.WriteLine($"-----monster.dat");
            System.Console.WriteLine($"-----packet.txt");
            System.Console.BackgroundColor = System.ConsoleColor.Blue;
            System.Console.WriteLine("-----map");
            System.Console.ResetColor();
            System.Console.WriteLine("----------0");
            System.Console.WriteLine("----------1");
            System.Console.WriteLine("----------...");
            Logger.Log.Warn(Language.Instance.GetMessageFromKey("ENTER_PATH"));
            string folder = System.Console.ReadLine();
            ImportFactory factory = new ImportFactory(folder);

            factory.ImportPackets();

            //Confirmation

            System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_ALL")} [Y/n]");
            System.ConsoleKeyInfo key = System.Console.ReadKey(true);
            if (key.KeyChar != 'n')
            {
                factory.ImportMaps();
                factory.loadMaps();
                factory.ImportPortals();
                factory.ImportNpcMonsters();
                factory.ImportMapNpcs();
                factory.ImportMonsters();
                factory.ImportShops();
                factory.ImportItems();
                factory.ImportShopItems();
            }
            else
            {
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MAPS")} [Y/n]");
                System.ConsoleKeyInfo key1 = System.Console.ReadKey(true);
                if (key1.KeyChar != 'n')
                {
                    factory.ImportMaps();
                }
                factory.loadMaps();
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_PORTALS")} [Y/n]");
                System.ConsoleKeyInfo key2 = System.Console.ReadKey(true);
                if (key2.KeyChar != 'n')
                {
                    factory.ImportPortals();
                }
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_NPCS")} [Y/n]");
                System.ConsoleKeyInfo key3 = System.Console.ReadKey(true);
                if (key3.KeyChar != 'n')
                {
                    factory.ImportNpcMonsters();
                }
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MONSTERS")} [Y/n]");
                System.ConsoleKeyInfo key4 = System.Console.ReadKey(true);
                if (key3.KeyChar != 'n')
                {
                    factory.ImportMonsters();
                }
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_SHOPS")} [Y/n]");
                System.ConsoleKeyInfo key5 = System.Console.ReadKey(true);
                if (key4.KeyChar != 'n')
                {
                    factory.ImportShops();
                }
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_ITEMS")} [Y/n]");
                System.ConsoleKeyInfo key6 = System.Console.ReadKey(true);
                if (key5.KeyChar != 'n')
                {
                    factory.ImportItems();
                }
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_SHOPITEMS")} [Y/n]");
                System.ConsoleKeyInfo key7 = System.Console.ReadKey(true);
                if (key6.KeyChar != 'n')
                {
                    factory.ImportShopItems();
                }
            }
            System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("DONE")}");
            Thread.Sleep(5000);
        }

        #endregion
    }
}