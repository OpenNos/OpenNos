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
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace OpenNos.Import.Console
{
    public class Program
    {
        #region Methods

        public static void Main(string[] args)
        {
            // initialize logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            System.Console.Title = $@"OpenNos Import Console v{fileVersionInfo.ProductVersion}dev";
            string text = $"IMPORT CONSOLE VERSION {fileVersionInfo.ProductVersion} by OpenNos Team";
            int offset = System.Console.WindowWidth / 2 + text.Length / 2;
            string separator = new string('=', System.Console.WindowWidth);
            System.Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
            if (DataAccessHelper.Initialize())
            {
                RegisterMappings();
            }
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            Logger.Log.Warn(Language.Instance.GetMessageFromKey("NEED_TREE"));
            System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.WriteLine(@"Root");
            System.Console.ResetColor();
            System.Console.WriteLine($@"-----_code_{ConfigurationManager.AppSettings["Language"]}_Card.txt");
            System.Console.WriteLine($@"-----_code_{ConfigurationManager.AppSettings["Language"]}_Item.txt");
            System.Console.WriteLine($@"-----_code_{ConfigurationManager.AppSettings["Language"]}_MapIDData.txt");
            System.Console.WriteLine($@"-----_code_{ConfigurationManager.AppSettings["Language"]}_monster.txt");
            System.Console.WriteLine($@"-----_code_{ConfigurationManager.AppSettings["Language"]}_Skill.txt");
            System.Console.WriteLine(@"-----packet.txt");
            System.Console.WriteLine(@"-----Card.dat");
            System.Console.WriteLine(@"-----Item.dat");
            System.Console.WriteLine(@"-----MapIDData.dat");
            System.Console.WriteLine(@"-----monster.dat");
            System.Console.WriteLine(@"-----Skill.dat");
            System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.WriteLine(@"-----map");
            System.Console.ResetColor();
            System.Console.WriteLine(@"----------0");
            System.Console.WriteLine(@"----------1");
            System.Console.WriteLine(@"----------...");

            try
            {
                Logger.Log.Warn(Language.Instance.GetMessageFromKey("ENTER_PATH"));
                string folder = string.Empty;
                if (args.Length == 0)
                {
                    folder = System.Console.ReadLine();
                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_ALL")} [Y/n]");
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
                    factory.ImportRespawnMapType();
                    factory.ImportMapType();
                    factory.ImportMapTypeMap();
                    ImportFactory.ImportAccounts();
                    factory.ImportPortals();
                    factory.ImportScriptedInstances();
                    factory.ImportItems();
                    factory.ImportSkills();
                    factory.ImportCards();
                    factory.ImportNpcMonsters();
                    factory.ImportNpcMonsterData();
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
                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_MAPS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMaps();
                        factory.LoadMaps();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_MAPTYPES")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMapType();
                        factory.ImportMapTypeMap();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_ACCOUNTS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        ImportFactory.ImportAccounts();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_PORTALS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportPortals();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_TIMESPACES")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportScriptedInstances();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_ITEMS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportItems();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_NPCMONSTERS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportNpcMonsters();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_NPCMONSTERDATA")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportNpcMonsterData();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_CARDS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportCards();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_SKILLS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportSkills();
                    }
                    
                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_MAPNPCS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMapNpcs();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_MONSTERS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportMonsters();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_SHOPS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportShops();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_TELEPORTERS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportTeleporters();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_SHOPITEMS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportShopItems();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_SHOPSKILLS")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportShopSkills();
                    }

                    System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("PARSE_RECIPES")} [Y/n]");
                    key = System.Console.ReadKey(true);
                    if (key.KeyChar != 'n')
                    {
                        factory.ImportRecipe();
                    }
                }
                System.Console.WriteLine($@"{Language.Instance.GetMessageFromKey("DONE")}");
                Thread.Sleep(5000);
            }
            catch (FileNotFoundException)
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
            DAOFactory.BCardDAO.RegisterMapping(typeof(BCardDTO)).InitializeMapper();
            DAOFactory.RespawnMapTypeDAO.RegisterMapping(typeof(RespawnMapTypeDTO)).InitializeMapper();
            DAOFactory.ShopDAO.RegisterMapping(typeof(ShopDTO)).InitializeMapper();
            DAOFactory.ShopItemDAO.RegisterMapping(typeof(ShopItemDTO)).InitializeMapper();
            DAOFactory.ShopSkillDAO.RegisterMapping(typeof(ShopSkillDTO)).InitializeMapper();
            DAOFactory.CardDAO.RegisterMapping(typeof(CardDTO)).InitializeMapper();
            DAOFactory.MateDAO.RegisterMapping(typeof(MateDTO)).InitializeMapper();
            DAOFactory.SkillDAO.RegisterMapping(typeof(SkillDTO)).InitializeMapper();
            DAOFactory.TeleporterDAO.RegisterMapping(typeof(TeleporterDTO)).InitializeMapper();
            DAOFactory.ScriptedInstanceDAO.RegisterMapping(typeof(ScriptedInstanceDTO)).InitializeMapper();
        }

        #endregion
    }
}