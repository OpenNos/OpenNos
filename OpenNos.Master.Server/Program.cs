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

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using log4net;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.Master.Library.Interface;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace OpenNos.Master.Server
{
    internal class Program
    {
        #region Members

        private static ManualResetEvent run = new ManualResetEvent(true);

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            try
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                Console.Title = $"OpenNos Master Server v{fileVersionInfo.ProductVersion}dev";
                string ipAddress = ConfigurationManager.AppSettings["MasterIP"];
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
                string text = $"MASTER SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                int offset = Console.WindowWidth / 2 + text.Length / 2;
                string separator = new string('=', Console.WindowWidth);
                Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);

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

                    // configure Services and Service Host
                    IScsServiceApplication _server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));

                    _server.AddService<ICommunicationService, CommunicationService>(new CommunicationService());
                    _server.ClientConnected += OnClientConnected;
                    _server.ClientDisconnected += OnClientDisconnected;

                    _server.Start();
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("STARTED"));
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

        private static void OnClientConnected(object sender, ServiceClientEventArgs e)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + e.Client.ClientId);
        }

        private static void OnClientDisconnected(object sender, ServiceClientEventArgs e)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + e.Client.ClientId);
        }

        private static void RegisterMappings()
        {
            //Prepare mappings for future use

            // register mappings for items
            DAOFactory.IteminstanceDAO.RegisterMapping(typeof(BoxInstance));
            DAOFactory.IteminstanceDAO.RegisterMapping(typeof(SpecialistInstance));
            DAOFactory.IteminstanceDAO.RegisterMapping(typeof(WearableInstance));
            DAOFactory.IteminstanceDAO.InitializeMapper(typeof(ItemInstance));

            // entities
            DAOFactory.AccountDAO.RegisterMapping(typeof(Account)).InitializeMapper();
            DAOFactory.CellonOptionDAO.RegisterMapping(typeof(CellonOptionDTO)).InitializeMapper();
            DAOFactory.CharacterDAO.RegisterMapping(typeof(Character)).InitializeMapper();
            DAOFactory.CharacterRelationDAO.RegisterMapping(typeof(CharacterRelationDTO)).InitializeMapper();
            DAOFactory.CharacterSkillDAO.RegisterMapping(typeof(CharacterSkill)).InitializeMapper();
            DAOFactory.ComboDAO.RegisterMapping(typeof(ComboDTO)).InitializeMapper();
            DAOFactory.DropDAO.RegisterMapping(typeof(DropDTO)).InitializeMapper();
            DAOFactory.GeneralLogDAO.RegisterMapping(typeof(GeneralLogDTO)).InitializeMapper();
            DAOFactory.ItemDAO.RegisterMapping(typeof(ItemDTO)).InitializeMapper();
            DAOFactory.BazaarItemDAO.RegisterMapping(typeof(BazaarItemDTO)).InitializeMapper();
            DAOFactory.MailDAO.RegisterMapping(typeof(MailDTO)).InitializeMapper();
            DAOFactory.MapDAO.RegisterMapping(typeof(MapDTO)).InitializeMapper();
            DAOFactory.MapMonsterDAO.RegisterMapping(typeof(MapMonster)).InitializeMapper();
            DAOFactory.MapNpcDAO.RegisterMapping(typeof(MapNpc)).InitializeMapper();
            DAOFactory.FamilyDAO.RegisterMapping(typeof(FamilyDTO)).InitializeMapper();
            DAOFactory.FamilyCharacterDAO.RegisterMapping(typeof(FamilyCharacterDTO)).InitializeMapper();
            DAOFactory.FamilyLogDAO.RegisterMapping(typeof(FamilyLogDTO)).InitializeMapper();
            DAOFactory.MapTypeDAO.RegisterMapping(typeof(MapTypeDTO)).InitializeMapper();
            DAOFactory.MapTypeMapDAO.RegisterMapping(typeof(MapTypeMapDTO)).InitializeMapper();
            DAOFactory.NpcMonsterDAO.RegisterMapping(typeof(NpcMonster)).InitializeMapper();
            DAOFactory.NpcMonsterSkillDAO.RegisterMapping(typeof(NpcMonsterSkill)).InitializeMapper();
            DAOFactory.PenaltyLogDAO.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
            DAOFactory.PortalDAO.RegisterMapping(typeof(PortalDTO)).InitializeMapper();
            DAOFactory.PortalDAO.RegisterMapping(typeof(Portal)).InitializeMapper();
            DAOFactory.QuicklistEntryDAO.RegisterMapping(typeof(QuicklistEntryDTO)).InitializeMapper();
            DAOFactory.RecipeDAO.RegisterMapping(typeof(Recipe)).InitializeMapper();
            DAOFactory.RecipeItemDAO.RegisterMapping(typeof(RecipeItemDTO)).InitializeMapper();
            DAOFactory.MinilandObjectDAO.RegisterMapping(typeof(MinilandObjectDTO)).InitializeMapper();
            DAOFactory.MinilandObjectDAO.RegisterMapping(typeof(MinilandObject)).InitializeMapper();
            DAOFactory.RespawnDAO.RegisterMapping(typeof(RespawnDTO)).InitializeMapper();
            DAOFactory.RespawnMapTypeDAO.RegisterMapping(typeof(RespawnMapTypeDTO)).InitializeMapper();
            DAOFactory.ShopDAO.RegisterMapping(typeof(Shop)).InitializeMapper();
            DAOFactory.ShopItemDAO.RegisterMapping(typeof(ShopItemDTO)).InitializeMapper();
            DAOFactory.ShopSkillDAO.RegisterMapping(typeof(ShopSkillDTO)).InitializeMapper();
            DAOFactory.CardDAO.RegisterMapping(typeof(CardDTO)).InitializeMapper();
            DAOFactory.BCardDAO.RegisterMapping(typeof(BCardDTO)).InitializeMapper();
            DAOFactory.SkillDAO.RegisterMapping(typeof(Skill)).InitializeMapper();
            DAOFactory.MateDAO.RegisterMapping(typeof(MateDTO)).InitializeMapper();
            DAOFactory.MateDAO.RegisterMapping(typeof(Mate)).InitializeMapper();
            DAOFactory.TeleporterDAO.RegisterMapping(typeof(TeleporterDTO)).InitializeMapper();
            DAOFactory.StaticBonusDAO.RegisterMapping(typeof(StaticBonusDTO)).InitializeMapper();
            DAOFactory.FamilyDAO.RegisterMapping(typeof(Family)).InitializeMapper();
            DAOFactory.FamilyCharacterDAO.RegisterMapping(typeof(FamilyCharacter)).InitializeMapper();
            DAOFactory.ScriptedInstanceDAO.RegisterMapping(typeof(ScriptedInstanceDTO)).InitializeMapper();
            DAOFactory.ScriptedInstanceDAO.RegisterMapping(typeof(ScriptedInstance)).InitializeMapper();
        }

        #endregion
    }
}