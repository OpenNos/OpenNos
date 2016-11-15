﻿/*
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
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Import.Console
{
    public class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            // initialize logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            System.Console.Title = $"OpenNos Import Console v{fileVersionInfo.ProductVersion}";
            string text = $"IMPORT CONSOLE VERSION {fileVersionInfo.ProductVersion} by OpenNos Team";
            if (args.Length == 0)
            {
                int offset = (System.Console.WindowWidth - text.Length) / 2;
                System.Console.WriteLine(new String('=', System.Console.WindowWidth));
                System.Console.SetCursorPosition(offset < 0 ? 0 : offset, System.Console.CursorTop);
                System.Console.WriteLine(text + "\n" +
                new String('=', System.Console.WindowWidth) + "\n");
            }

            if (DataAccessHelper.Initialize())
            {
                RegisterMappings();
            }
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            Logger.Log.Warn(Language.Instance.GetMessageFromKey("NEED_TREE"));
            System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.WriteLine("Root");
            System.Console.ResetColor();

            // System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_BCard.txt");
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_Item.txt");
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_MapIDData.txt");
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_monster.txt");
            System.Console.WriteLine($"-----_code_{System.Configuration.ConfigurationManager.AppSettings["language"]}_Skill.txt");

            // System.Console.WriteLine("-----BCard.dat");
            System.Console.WriteLine("-----Item.dat");
            System.Console.WriteLine("-----MapIDData.dat");
            System.Console.WriteLine("-----monster.dat");
            System.Console.WriteLine("-----Skill.dat");
            System.Console.WriteLine("-----packet.txt");
            System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.WriteLine("-----map");
            System.Console.ResetColor();
            System.Console.WriteLine("----------0");
            System.Console.WriteLine("----------1");
            System.Console.WriteLine("----------...");

            try
            {
                Logger.Log.Warn(Language.Instance.GetMessageFromKey("ENTER_PATH"));
                string folder = String.Empty;
                if (args.Length == 0)
                {
                    folder = System.Console.ReadLine();
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_ALL")} [Y/n]");
                    key = System.Console.ReadKey(true);
                }
                else
                {
                    foreach (string str in args)
                    {
                        folder += str + " ";
                    }
                }
                ImportFactory factory = new ImportFactory(folder);
                factory.ImportPackets();

                if (key.KeyChar != 'n')
                {
                    factory.ImportMaps();
                    factory.LoadMaps();
                    factory.ImportMapType();
                    factory.ImportMapTypeMap();
                    factory.ImportAccounts();
                    factory.ImportPortals();
                    factory.ImportItems();
                    factory.ImportSkills();
                    factory.ImportNpcMonsters();
                    factory.ImportMapNpcs();
                    factory.ImportMonsters();
                    factory.ImportShops();
                    factory.ImportTeleporters();
                    factory.ImportShopItems();
                    factory.ImportShopSkills();
                    factory.ImportRecipe();
                }
                else
                {
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MAPS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMaps();
                        factory.LoadMaps();
                    }

                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MAPTYPES")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMapType();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MAPTYPEMAPS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMapTypeMap();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_ACCOUNTS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportAccounts();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_PORTALS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportPortals();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_ITEMS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportItems();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_NPCS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportNpcMonsters();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_SKILLS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportSkills();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MAPNPCS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMapNpcs();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_MONSTERS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMonsters();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_SHOPS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportShops();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_TELEPORTERS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportTeleporters();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_SHOPITEMS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportShopItems();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_SHOPSKILLS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportShopSkills();
                    }
                    System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("PARSE_RECIPES")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportRecipe();
                    }
                }
                System.Console.WriteLine($"{Language.Instance.GetMessageFromKey("DONE")}");
                Thread.Sleep(5000);
            }
            catch (System.IO.FileNotFoundException)
            {
                Logger.Log.Error(Language.Instance.GetMessageFromKey("AT_LEAST_ONE_FILE_MISSING"));
                Thread.Sleep(5000);
            }
        }

        private static void RegisterMappings()
        {
            // entities
            DAOFactory.AccountDAO.RegisterMapping(typeof(AccountDTO)).InitializeMapper();
            DAOFactory.CellonOptionDAO.RegisterMapping(typeof(CellonOptionDTO)).InitializeMapper();
            DAOFactory.CharacterDAO.RegisterMapping(typeof(CharacterDTO)).InitializeMapper();
            DAOFactory.CharacterSkillDAO.RegisterMapping(typeof(CharacterSkillDTO)).InitializeMapper();
            DAOFactory.ComboDAO.RegisterMapping(typeof(ComboDTO)).InitializeMapper();
            DAOFactory.DropDAO.RegisterMapping(typeof(DropDTO)).InitializeMapper();
            DAOFactory.GeneralLogDAO.RegisterMapping(typeof(GeneralLogDTO)).InitializeMapper();
            DAOFactory.ItemDAO.RegisterMapping(typeof(ItemDTO)).InitializeMapper();
            DAOFactory.MailDAO.RegisterMapping(typeof(MailDTO)).InitializeMapper();
            DAOFactory.MapDAO.RegisterMapping(typeof(MapDTO)).InitializeMapper();
            DAOFactory.MapMonsterDAO.RegisterMapping(typeof(MapMonsterDTO)).InitializeMapper();
            DAOFactory.MapNpcDAO.RegisterMapping(typeof(MapNpcDTO)).InitializeMapper();
            DAOFactory.MapTypeDAO.RegisterMapping(typeof(MapTypeDTO)).InitializeMapper();
            DAOFactory.MapTypeMapDAO.RegisterMapping(typeof(MapTypeMapDTO)).InitializeMapper();
            DAOFactory.NpcMonsterDAO.RegisterMapping(typeof(NpcMonsterDTO)).InitializeMapper();
            DAOFactory.NpcMonsterSkillDAO.RegisterMapping(typeof(NpcMonsterSkillDTO)).InitializeMapper();
            DAOFactory.PenaltyLogDAO.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
            DAOFactory.PortalDAO.RegisterMapping(typeof(PortalDTO)).InitializeMapper();
            DAOFactory.QuicklistEntryDAO.RegisterMapping(typeof(QuicklistEntryDTO)).InitializeMapper();
            DAOFactory.RecipeDAO.RegisterMapping(typeof(RecipeDTO)).InitializeMapper();
            DAOFactory.RecipeItemDAO.RegisterMapping(typeof(RecipeItemDTO)).InitializeMapper();
            DAOFactory.RespawnDAO.RegisterMapping(typeof(RespawnDTO)).InitializeMapper();
            DAOFactory.ShopDAO.RegisterMapping(typeof(ShopDTO)).InitializeMapper();
            DAOFactory.ShopItemDAO.RegisterMapping(typeof(ShopItemDTO)).InitializeMapper();
            DAOFactory.ShopSkillDAO.RegisterMapping(typeof(ShopSkillDTO)).InitializeMapper();
            DAOFactory.SkillDAO.RegisterMapping(typeof(SkillDTO)).InitializeMapper();
            DAOFactory.TeleporterDAO.RegisterMapping(typeof(TeleporterDTO)).InitializeMapper();
        }

        #endregion
    }
}