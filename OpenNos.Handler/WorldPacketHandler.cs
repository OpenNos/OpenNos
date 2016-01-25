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
using OpenNos.Core;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenNos.ServiceRef.Internal;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Reflection;

namespace OpenNos.Handler
{
    public class WorldPacketHandler
    {

        #region Instantiation

        private readonly ClientSession _session;

        public ClientSession Session
        {
            get { return _session; }
        }

        public WorldPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Methods

        public void GetStartupInventory()
        {
            foreach (String inv in Session.Character.GenerateStartupInventory())
            {
                Session.Client.SendPacket(inv);
            }
        }

        #endregion

        #region CharacterSelection

        [Packet("Char_DEL")]
        public void DeleteCharacter(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(Session.SessionId);
            if (account.Password == OpenNos.Core.EncryptionBase.sha256(packetsplit[3]))
            {
                DAOFactory.GeneralLogDAO.SetCharIdNull((long?)Convert.ToInt64(DAOFactory.CharacterDAO.LoadBySlot(account.AccountId, Convert.ToByte(packetsplit[2])).CharacterId));
                DAOFactory.CharacterDAO.Delete(account.AccountId, Convert.ToByte(packetsplit[2]));
                LoadCharacters(packet);
            }
            else
            {
                Session.Client.SendPacket(String.Format("info {0}", Language.Instance.GetMessageFromKey("BAD_PASSWORD")));
            }

        }
        [Packet("Char_NEW")]
        public void CreateCharacter(string packet)
        {
            //todo, hold Account Information in Authorized object
            long accountId = Session.Account.AccountId;
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2].Length > 3 && packetsplit[2].Length < 15)
            {

                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) == null)
                {
                    Random r = new Random();
                    CharacterDTO newCharacter = new CharacterDTO()
                    {
                        Class = (byte)ClassType.Adventurer,
                        Gender = Convert.ToByte(packetsplit[4]),
                        Gold = 10000,
                        HairColor = Convert.ToByte(packetsplit[6]),
                        HairStyle = Convert.ToByte(packetsplit[5]),
                        Hp = 221,
                        JobLevel = 1,
                        JobLevelXp = 0,
                        Level = 1,
                        LevelXp = 0,
                        MapId = 1,
                        MapX = (short)(r.Next(77, 82)),
                        MapY = (short)(r.Next(112, 120)),
                        Mp = 221,
                        Name = packetsplit[2],
                        Slot = Convert.ToByte(packetsplit[3]),
                        AccountId = accountId,
                        StateEnum = CharacterState.Active,

                    };

                    SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                    LoadCharacters(packet);
                }

                else Session.Client.SendPacketFormat("info {0}", Language.Instance.GetMessageFromKey("ALREADY_TAKEN"));
            }
        }
        /// <summary>
        /// Load Characters, this is the Entrypoint for the Client, Wait for 3 Packets.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        [Packet("OpenNos.EntryPoint", 3)]
        public void LoadCharacters(string packet)
        {
            string[] loginPacketParts = packet.Split(' ');

            //load account by given SessionId
            if (Session.Account == null)
            {
                bool hasRegisteredAccountLogin = true;
                try
                {
                    hasRegisteredAccountLogin = ServiceFactory.Instance.CommunicationService.HasRegisteredAccountLogin(loginPacketParts[4], Session.SessionId);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                }

                if (loginPacketParts.Length > 4 && hasRegisteredAccountLogin)
                {
                    AccountDTO accountDTO = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);

                    if (accountDTO != null)
                    {
                        if (accountDTO.Password.Equals(EncryptionBase.sha256(loginPacketParts[6])))
                        {
                            var account = new GameObject.Account()
                            {
                                AccountId = accountDTO.AccountId,
                                Name = accountDTO.Name,
                                Password = accountDTO.Password,
                                Authority = accountDTO.Authority
                            };

                            Session.InitializeAccount(account);
                        }
                        else
                        {
                            Logger.Log.ErrorFormat("Client {0} forced Disconnection, invalid Password or SessionId.", Session.Client.ClientId);
                            Session.Client.Disconnect();
                        }
                    }
                    else
                    {
                        Logger.Log.ErrorFormat("Client {0} forced Disconnection, invalid AccountName.", Session.Client.ClientId);
                        Session.Client.Disconnect();
                    }
                }
                else
                {
                    Logger.Log.ErrorFormat("Client {0} forced Disconnection, login has not been registered or Account has already logged in.", Session.Client.ClientId);
                    Session.Client.Disconnect();
                    Session.Destroy();
                    return;
                }
            }

            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
            Logger.Log.InfoFormat(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), Session.SessionId);
            Session.Client.SendPacket("clist_start 0");
            foreach (CharacterDTO character in characters)
            {
                //move to character
                InventoryItemDTO[] item = new InventoryItemDTO[15];
                for (short i = 0; i < 15; i++)
                {
                    InventoryDTO inv = DAOFactory.InventoryDAO.LoadBySlotAndType(character.CharacterId, i, (short)InventoryType.Equipment);
                    if (inv != null)
                    {
                        item[i] = DAOFactory.InventoryItemDAO.LoadById(inv.InventoryItemId);

                    }

                }

                Session.Client.SendPacket(String.Format("clist {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}.{10}.{11}.{12}.{13}.{14}.{15}.{16} {17} {18} {19} {20}.{21} {22} {23}",
                    character.Slot, character.Name, 0, character.Gender, character.HairStyle, character.HairColor, 5, character.Class, character.Level, item[(short)EquipmentType.Hat] != null ? item[(short)EquipmentType.Hat].ItemVNum : 0, item[(short)EquipmentType.Armor] != null ? item[(short)EquipmentType.Armor].ItemVNum : 0, item[(short)EquipmentType.MainWeapon] != null ? item[(short)EquipmentType.MainWeapon].ItemVNum : 0, item[(short)EquipmentType.SecondaryWeapon] != null ? item[(short)EquipmentType.SecondaryWeapon].ItemVNum : 0, item[(short)EquipmentType.Mask] != null ? item[(short)EquipmentType.Mask].ItemVNum : 0, item[(short)EquipmentType.Fairy] != null ? item[(short)EquipmentType.Fairy].ItemVNum : 0, item[(short)EquipmentType.CostumeSuite] != null ? item[(short)EquipmentType.CostumeSuite].ItemVNum : 0, item[(short)EquipmentType.CostumeHat] != null ? item[(short)EquipmentType.CostumeHat].ItemVNum : 0, 1, 0, 0, -1, -1, item[(short)EquipmentType.Hat] != null ? (ServerManager.GetItem(item[(short)EquipmentType.Hat].ItemVNum).Colored ? item[(short)EquipmentType.Hat].Color : character.HairColor) : character.HairColor, 0));
            }
            Session.Client.SendPacket("clist_end");

        }

        [Packet("select")]
        public void SelectCharacter(string packet)
        {
            try
            {
                string[] packetsplit = packet.Split(' ');
                CharacterDTO characterDTO = DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, Convert.ToByte(packetsplit[2]));
                if (characterDTO != null)
                    Session.Character = new GameObject.Character()
                    {
                        AccountId = characterDTO.AccountId,
                        CharacterId = characterDTO.CharacterId,
                        Class = characterDTO.Class,
                        Dignite = characterDTO.Dignite,
                        Gender = characterDTO.Gender,
                        Gold = characterDTO.Gold,
                        HairColor = characterDTO.HairColor,
                        HairStyle = characterDTO.HairStyle,
                        Hp = characterDTO.Hp,
                        JobLevel = characterDTO.JobLevel,
                        JobLevelXp = characterDTO.JobLevelXp,
                        Level = characterDTO.Level,
                        LevelXp = characterDTO.LevelXp,
                        MapId = characterDTO.MapId,
                        MapX = characterDTO.MapX,
                        MapY = characterDTO.MapY,
                        Mp = characterDTO.Mp,
                        State = characterDTO.State,
                        Faction = characterDTO.Faction,
                        Name = characterDTO.Name,
                        Reput = characterDTO.Reput,
                        Slot = characterDTO.Slot,
                        Authority = Session.Account.Authority,
                        SpAdditionPoint = characterDTO.SpAdditionPoint,
                        SpPoint = characterDTO.SpPoint,
                        LastPulse = 0,
                        LastPortal = 0,
                        LastSp = 0,
                        Invisible = 0,
                        ArenaWinner = 0,
                        Morph = 0,
                        MorphUpgrade = 0,
                        MorphUpgrade2 = 0,
                        Direction = 0,
                        Rested = 0,
                        BackPack = characterDTO.Backpack,
                        Speed = ServersData.SpeedData[characterDTO.Class],
                        Compliment = characterDTO.Compliment
                    };

                Session.Character.Update();
                Session.Character.LoadInventory();
                DAOFactory.AccountDAO.WriteConnectionLog(Session.Character.AccountId, Session.Client.RemoteEndPoint.ToString(), Session.Character.CharacterId, "Connexion", "World");
                Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);
                Session.RegisterForMapNotification();
                Session.Client.SendPacket("OK");
                Session.HealthThread = new Thread(new ThreadStart(healthThread));

                if (Session.HealthThread != null && !Session.HealthThread.IsAlive)
                    Session.HealthThread.Start();

                //inform everyone about connected character
                ServiceFactory.Instance.CommunicationService.ConnectCharacter(Session.Character.Name, Session.Account.Name);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        #endregion

        #region Map
        [Packet("pulse")]
        public void Pulse(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            Session.Character.LastPulse += 60;
            if (Convert.ToInt32(packetsplit[2]) != Session.Character.LastPulse)
            {
                Session.Client.Disconnect();
            }
        }
        [Packet("say")]
        public void Say(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";
            message.Trim();

            ClientLinkManager.Instance.Broadcast(Session,
                Session.Character.GenerateSay(message, 0),
                ReceiverType.AllOnMapExceptMe);
        }
        [Packet("walk")]
        public void Walk(string packet)
        {
            if (Session.Character.ThreadCharChange != null && Session.Character.ThreadCharChange.IsAlive)
                Session.Character.ThreadCharChange.Abort();

            string[] packetsplit = packet.Split(' ');

            Session.Character.MapX = Convert.ToInt16(packetsplit[2]);
            Session.Character.MapY = Convert.ToInt16(packetsplit[3]);

            ClientLinkManager.Instance.Broadcast(Session,
              Session.Character.GenerateMv(Session.Character.MapX, Session.Character.MapY),
                ReceiverType.AllOnMapExceptMe);
            Session.Client.SendPacket(Session.Character.GenerateCond());

        }
        [Packet("guri")]
        public void Guri(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2] == "10" && Convert.ToInt32(packetsplit[5]) >= 973 && Convert.ToInt32(packetsplit[5]) <= 999)
            {

                Session.Client.SendPacket(Session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099),
                    ReceiverType.AllOnMap);
            }
        }
        [Packet("preq")]
        public void Preq(string packet)
        {
            bool teleported = false;
            double def = (((TimeSpan)(DateTime.Now - new DateTime(2010, 1, 1, 0, 0, 0))).TotalSeconds) - (Session.Character.LastPortal);
            if (def >= 4)
            {
                foreach (Portal portal in ServerManager.GetMap(Session.Character.MapId).Portals)
                {
                    if (!teleported && Session.Character.MapY >= portal.SourceY - 1 && Session.Character.MapY <= portal.SourceY + 1 && Session.Character.MapX >= portal.SourceX - 1 && Session.Character.MapX <= portal.SourceX + 1)
                    {
                        Session.Character.MapId = portal.DestinationMapId;
                        Session.Character.MapX = portal.DestinationX;
                        Session.Character.MapY = portal.DestinationY;

                        Session.Character.LastPortal = (((TimeSpan)(DateTime.Now - new DateTime(2010, 1, 1, 0, 0, 0))).TotalSeconds);
                        MapOut();
                        ChangeMap();
                        teleported = true;
                    }
                }

            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));

            }
        }
        [Packet("rest")]
        public void Rest(string packet)
        {
            Session.Character.Rested = Session.Character.Rested == 1 ? 0 : 1;
            if (Session.Character.IsVehicled)
                Session.Character.Rested = 0;
            if (Session.Character.ThreadCharChange != null && Session.Character.ThreadCharChange.IsAlive)
                Session.Character.ThreadCharChange.Abort();


            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);

        }
        [Packet("dir")]
        public void Dir(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (Convert.ToInt32(packetsplit[4]) == Session.Character.CharacterId)
            {
                Session.Character.Direction = Convert.ToInt32(packetsplit[2]);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateDir(), ReceiverType.AllOnMap);

            }
        }
        [Packet("u_s")]
        public void UseSkill(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            ClientLinkManager.Instance.Broadcast(Session, String.Format("cancel 2 {0}", packetsplit[4]), ReceiverType.OnlyMe);

        }
        [Packet("ncif")]
        public void GetNamedCharacterInformation(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (packetsplit[2] == "1")
            {
                ClientLinkManager.Instance.RequiereBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateStatInfo");
            }
            if (packetsplit[2] == "2")
            {
                foreach (Npc npc in ServerManager.GetMap(Session.Character.MapId).Npcs)
                    if (npc.NpcId == Convert.ToInt16(packetsplit[3]))
                        ClientLinkManager.Instance.Broadcast(Session, String.Format("st 2 {0} {1} 100 100 50000 50000", packetsplit[3], npc.Level), ReceiverType.OnlyMe);
            }
        }
        [Packet("game_start")]
        public void StartGame(string packet)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & DAOFactory.GeneralLogDAO.LoadByLogType("Connexion", Session.Character.CharacterId).Count() == 1)
                Session.Client.SendPacket("scene 40");
            if (System.Configuration.ConfigurationManager.AppSettings["WorldInformation"].ToLower() == "true")
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("-----------World Information-----------", 10));
                Assembly assembly = Assembly.GetEntryAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format("OpenNos by OpenNos Team\nVersion: v{0}", fileVersionInfo.ProductVersion), 6));
                Session.Client.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
            }

            Session.Client.SendPacket(Session.Character.GenerateTit());
            ChangeMap();
            Session.Client.SendPacket("rank_cool 0 0 18000");//TODO add rank cool

            Session.Client.SendPacket("scr 0 0 0 0 0 0");

            Session.Client.SendPacket(String.Format("bn 0 {0}", Language.Instance.GetMessageFromKey("BN0")));
            Session.Client.SendPacket(String.Format("bn 1 {0}", Language.Instance.GetMessageFromKey("BN1")));
            Session.Client.SendPacket(String.Format("bn 2 {0}", Language.Instance.GetMessageFromKey("BN2")));
            Session.Client.SendPacket(String.Format("bn 3 {0}", Language.Instance.GetMessageFromKey("BN3")));
            Session.Client.SendPacket(String.Format("bn 4 {0}", Language.Instance.GetMessageFromKey("BN4")));
            Session.Client.SendPacket(String.Format("bn 5 {0}", Language.Instance.GetMessageFromKey("BN5")));
            Session.Client.SendPacket(String.Format("bn 6 {0}", Language.Instance.GetMessageFromKey("BN6")));

            Session.Client.SendPacket(Session.Character.GenerateExts());
            Session.Client.SendPacket(Session.Character.GenerateGold());

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpPoint(), ReceiverType.AllOnMap);
            GetStartupInventory();
            //gidx
            Session.Client.SendPacket("mlinfo 3800 2000 100 0 0 10 0 Mélodie^du^printemps Bienvenue");
            //cond
            Session.Client.SendPacket("p_clear");
            //sc_p pet
            Session.Client.SendPacket("pinit 0");
            Session.Client.SendPacket("zzim");
            Session.Client.SendPacket(String.Format("twk 1 {0} {1} {2} shtmxpdlfeoqkr", Session.Character.CharacterId, Session.Account.Name, Session.Character.Name));


        }
        [Packet("remove")]
        public void un
            (string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                short type = (short)InventoryType.Equipment;
                short slot = 0; short.TryParse(packetsplit[2], out slot);

                Inventory inventory = Session.Character.EquipmentList.LoadBySlotAndType(slot, type);


                if (inventory != null)
                {
                    if (slot == (short)EquipmentType.Sp && Session.Character.UseSp)
                    {
                        Session.Character.LastSp = (((TimeSpan)(DateTime.Now - new DateTime(2010, 1, 1, 0, 0, 0))).TotalSeconds);
                        Thread removeSP = new Thread(() => RemoveSP(inventory.InventoryItem.ItemVNum));
                        removeSP.Start();
                    }
                    Inventory inv = Session.Character.InventoryList.CreateItem(inventory.InventoryItem, Session.Character);
                    if (inv != null)
                    {
                        short Slot = inv.Slot;
                        if (Slot != -1)
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inventory.InventoryItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, Slot, inventory.InventoryItem.Rare, inventory.InventoryItem.Color, inventory.InventoryItem.Upgrade));
                        Session.Character.EquipmentList.DeleteFromSlotAndType(slot, type);

                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                        Thread.Sleep(100);
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);

                    }
                }
            }
        }
        [Packet("wear")]
        public void Wear(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                short type = 0; short.TryParse(packetsplit[3], out type);
                short slot = 0; short.TryParse(packetsplit[2], out slot);
                Item iteminfo;

                Inventory inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);

                if (inventory != null)
                {
                    iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                    if (iteminfo.LevelMinimum <= Session.Character.Level && ((iteminfo.Classe >> Session.Character.Class) & 1) == 1)
                    {
                        if (!(Session.Character.UseSp && iteminfo.EquipmentSlot == (short)EquipmentType.Fairy && iteminfo.Element != ServerManager.GetItem(Session.Character.InventoryList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment).InventoryItem.ItemVNum).Element))
                        {
                            if (!(Session.Character.UseSp && iteminfo.EquipmentSlot == (short)EquipmentType.Sp))
                            {
                                if (Session.Character.JobLevel >= iteminfo.LevelJobMinimum)
                                {
                                    Inventory equip = Session.Character.EquipmentList.LoadBySlotAndType(iteminfo.EquipmentSlot, (short)InventoryType.Equipment);
                                    if (equip == null)
                                    {
                                        inventory.Type = (short)InventoryType.Equipment;
                                        inventory.Slot = iteminfo.EquipmentSlot;

                                        Session.Character.EquipmentList.InsertOrUpdate(ref inventory);
                                        DeleteItem(type, slot);

                                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                                        Thread.Sleep(100);
                                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                                    }
                                    else
                                    {
                                        inventory.Type = (short)InventoryType.Equipment;
                                        inventory.Slot = iteminfo.EquipmentSlot;

                                        equip.Slot = slot;
                                        equip.Type = type;

                                        DeleteItem(type, slot);
                                        Session.Character.EquipmentList.DeleteFromSlotAndType(slot, type);

                                        Session.Character.InventoryList.InsertOrUpdate(ref equip);
                                        Session.Character.EquipmentList.InsertOrUpdate(ref inventory);
                                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(equip.InventoryItem.ItemVNum, equip.InventoryItem.Amount, type, equip.Slot, equip.InventoryItem.Rare, equip.InventoryItem.Color, equip.InventoryItem.Upgrade));

                                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                                        Thread.Sleep(100);
                                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                                    }
                                }
                                else
                                {
                                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 0));
                                }
                            }
                            else
                            {
                                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SP_BLOCKED"), 10));
                            }
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                        }
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 0));
                    }
                }
            }
        }
        [Packet("n_run")]
        public void npcRunFunction(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 5)
            {
                short type; short.TryParse(packetsplit[3], out type);
                short runner; short.TryParse(packetsplit[2], out runner);
                short data3; short.TryParse(packetsplit[4], out data3);
                short npcid; short.TryParse(packetsplit[5], out npcid);
                switch (runner)
                {
                    case 1:
                        if (Session.Character.Class == (short)ClassType.Adventurer)
                        {
                            if (Session.Character.Level >= 15 && Session.Character.JobLevel >= 20)
                            {
                                {
                                    if (Session.Character.EquipmentList.isEmpty())
                                    {
                                        ClassChange(Convert.ToByte(type));
                                        byte joblevel;
                                        if (Byte.TryParse(packetsplit[2], out joblevel) && joblevel <= 80 && joblevel > 0)
                                        {
                                            Session.Character.JobLevel = 1;
                                            Session.Client.SendPacket(Session.Character.GenerateLev());
                                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                                        }
                                    }
                                    else
                                    {
                                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                                    }
                                }
                            }
                            else
                            {
                                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                            }
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        }
                        break;
                    case 2:
                        Session.Client.SendPacket(String.Format("wopen {0} 0", 1));
                        break;
                    case 10:
                        Session.Client.SendPacket(String.Format("wopen {0} 0", 3));
                        break;
                    case 12:
                        Session.Client.SendPacket(String.Format("wopen {0} 0", type));
                        break;
                    case 14:
                        //m_list 2 1002 1003 1004 1005 1006 1007 1008 1009 1010 180 181 2127 2178 1242 1243 1244 2504 2505 - 100
                        Session.Client.SendPacket(String.Format("wopen {0} 0", 27));
                        break;

                }

            }
        }
        [Packet("npinfo")]
        public void GetStats(string packet)
        {

            Session.Client.SendPacket(Session.Character.GenerateStatChar());
        }
        [Packet("put")]
        public void PutItem(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            short amount; short.TryParse(packetsplit[4], out amount);
            Inventory inv;
            Inventory invitem = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
            if (invitem != null && ServerManager.GetItem(invitem.InventoryItem.ItemVNum).Droppable == 1)
            {
                MapItem DroppedItem = Session.Character.InventoryList.PutItem(Session, type, slot, amount, out inv);
                if (inv.InventoryItem.Amount == 0)
                    DeleteItem(type, inv.Slot);
                ClientLinkManager.Instance.Broadcast(Session, String.Format("drop {0} {1} {2} {3} {4} {5} {6}", DroppedItem.ItemVNum, DroppedItem.InventoryItemId, DroppedItem.PositionX, DroppedItem.PositionY, DroppedItem.Amount, 0, -1), ReceiverType.AllOnMap);
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
            }
        }
        [Packet("sell")]
        public void SellShop(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 6)
            {
                short type; short.TryParse(packetsplit[4], out type);
                short slot; short.TryParse(packetsplit[5], out slot);
                short amount; short.TryParse(packetsplit[6], out amount);
                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                if (inv != null && amount <= inv.InventoryItem.Amount)
                {
                    if (ServerManager.GetItem(inv.InventoryItem.ItemVNum).Soldable == 1)
                    {
                        Item item = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                        Session.Character.Gold += item.Price * amount;
                        DeleteItem(type, slot);
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, String.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALIDE"), item.Name, amount)));
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"), 0));//need to see if on global it's a MSG packet^^
                    }
                }
            }

        }
        [Packet("buy")]
        public void BuyShop(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long owner; long.TryParse(packetsplit[3], out owner);
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[4], out slot);
            short amount; short.TryParse(packetsplit[5], out amount);
            if (type == 1)//usershop
            {
                KeyValuePair<long, MapShop> shop = Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(owner));
                PersonalShopItem item = shop.Value.Items.FirstOrDefault(i => i.Slot.Equals(slot));
                if (amount > item.Amount)
                    amount = item.Amount;
                if (item.Price * amount < Session.Character.Gold)
                {
                    Session.Character.Gold -= item.Price * amount;
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    InventoryItem newItem = new InventoryItem()
                    {
                        InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                        Amount = amount,
                        ItemVNum = item.ItemVNum,
                        Rare = item.Rare,
                        Upgrade = item.Upgrade,
                        Color = item.Color,
                        Concentrate = item.Concentrate,
                        CriticalLuckRate = item.CriticalLuckRate,
                        CriticalRate = item.CriticalRate,
                        DamageMaximum = item.DamageMaximum,
                        DamageMinimum = item.DamageMinimum,
                        DarkElement = item.DarkElement,
                        DistanceDefence = item.DistanceDefence,
                        DistanceDefenceDodge = item.DistanceDefenceDodge,
                        DefenceDodge = item.DefenceDodge,
                        ElementRate = item.ElementRate,
                        FireElement = item.FireElement,
                        HitRate = item.HitRate,
                        LightElement = item.LightElement,
                        MagicDefence = item.MagicDefence,
                        RangeDefence = item.RangeDefence,
                        SlDefence = item.SlDefence,
                        SpXp = item.SpXp,
                        SpLevel = item.SpLevel,
                        SlElement = item.SlElement,
                        SlHit = item.SlHit,
                        SlHP = item.SlHP,
                        WaterElement = item.WaterElement,
                    };

                    Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                    if (inv != null)
                    {
                        short Slot = inv.Slot;
                        if (Slot != -1)
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, Slot, newItem.Rare, newItem.Color, newItem.Upgrade));
                    }
                    ClientLinkManager.Instance.BuyValidate(Session, shop, slot, amount);
                    KeyValuePair<long, MapShop> shop2 = Session.CurrentMap.ShopUserList.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                    loadShopItem(owner, shop2);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                }

            }
            else
            {

                Npc npc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.NpcId.Equals((short)owner));

                ShopItem item = npc.Shop.ShopItems.FirstOrDefault(it => it.Slot.Equals(slot));
                long price = ServerManager.GetItem(item.ItemVNum).Price * amount;

                if (price > 0 && price <= Session.Character.Gold)
                {

                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, String.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALIDE"), ServerManager.GetItem(item.ItemVNum).Name, amount)));

                    Session.Character.Gold -= price;
                    Session.Client.SendPacket(Session.Character.GenerateGold());

                    InventoryItem newItem = new InventoryItem()
                    {
                        InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                        Amount = amount,
                        ItemVNum = item.ItemVNum,
                        Rare = item.Rare,
                        Upgrade = item.Upgrade,
                        Color = item.Color,
                        Concentrate = 0,
                        CriticalLuckRate = 0,
                        CriticalRate = 0,
                        DamageMaximum = 0,
                        DamageMinimum = 0,
                        DarkElement = 0,
                        DistanceDefence = 0,
                        DistanceDefenceDodge = 0,
                        DefenceDodge = 0,
                        ElementRate = 0,
                        FireElement = 0,
                        HitRate = 0,
                        LightElement = 0,
                        MagicDefence = 0,
                        RangeDefence = 0,
                        SpXp = 0,
                        SpLevel = 0,
                        SlDefence = 0,
                        SlElement = 0,
                        SlHit = 0,
                        SlHP = 0,
                        WaterElement = 0,
                    };

                    Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                    if (inv != null)
                    {
                        short Slot = inv.Slot;
                        if (Slot != -1)
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, Slot, newItem.Rare, newItem.Color, newItem.Upgrade));
                    }

                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                }
            }
        }
        [Packet("shopping")]
        public void Shopping(string packet)
        {
            //n_inv 2 1834 0 100 0.13.13.0.0.330 0.14.15.0.0.2299 0.18.120.0.0.3795 0.19.107.0.0.3795 0.20.94.0.0.3795 0.37.95.0.0.5643 0.38.97.0.0.11340 0.39.99.0.0.18564 0.48.108.0.0.5643 0.49.110.0.0.11340 0.50.112.0.0.18564 0.59.121.0.0.5643 0.60.123.0.0.11340 0.61.125.0.0.18564
            string[] packetsplit = packet.Split(' ');
            short NpcId; short.TryParse(packetsplit[5], out NpcId);
            short type; short.TryParse(packetsplit[2], out type);
            Npc npc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.NpcId.Equals(NpcId));
            string shoplist = String.Empty;
            Shop shop = npc.Shop;
            if (shop != null)
            {
                foreach (ShopItem item in shop.ShopItems.Where(s => s.Type.Equals(type)))
                {
                    Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                    if (iteminfo.Type != 0)
                        shoplist += String.Format(" {0}.{1}.{2}.{3}.{4}", iteminfo.Type, item.Slot, item.ItemVNum, -1, ServerManager.GetItem(item.ItemVNum).Price);
                    else
                        shoplist += String.Format(" {0}.{1}.{2}.{3}.{4}.{5}", iteminfo.Type, item.Slot, item.ItemVNum, item.Rare, iteminfo.Colored ? item.Color : item.Upgrade, ServerManager.GetItem(item.ItemVNum).Price);

                }

                Session.Client.SendPacket(String.Format("n_inv 2 {0} 0 0{1}", npc.NpcId, shoplist));
            }
        }
        [Packet("npc_req")]
        public void ShowShop(string packet)
        {
            //n_inv 1 2 0 0 0.0.302.7.0.990000. 0.1.264.5.6.2500000. 0.2.69.7.0.650000. 0.3.4106.0.0.4200000. -1 0.5.4240.0.0.11200000. 0.6.4240.0.5.24000000. 0.7.4801.0.0.6200000. 0.8.4240.0.10.32000000. 0.9.712.0.3.250000. 0.10.997.0.4.250000. 1.11.1895.4.16000.-1.-1 1.12.1897.6.18000.-1.-1 -1 1.14.1902.3.35000.-1.-1 1.15.1237.2.12000.-1.-1 -1 -1 1.18.1249.3.92000.-1.-1 0.19.4240.0.1.10500000. -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                int mode; int.TryParse(packetsplit[2], out mode);
                if (mode == 1)//personnal
                {
                    if (packetsplit.Length > 3)
                    {
                        long owner; long.TryParse(packetsplit[3], out owner);

                        KeyValuePair<long, MapShop> shop = Session.CurrentMap.ShopUserList.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                        loadShopItem(owner, shop);
                    }


                }
                else
                {
                    Npc npc = ServerManager.GetMap(Session.Character.MapId).Npcs.FirstOrDefault(n => n.NpcId.Equals(Convert.ToInt16(packetsplit[3])));
                    if (npc.GetNpcDialog() != String.Empty)
                        Session.Client.SendPacket(npc.GetNpcDialog());



                }
            }
        }
        [Packet("m_shop")]
        public void createShop(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short[] type = new short[20];
            long[] gold = new long[20];
            short[] slot = new short[20];
            short[] qty = new short[20];

            string shopname = "";
            if (packetsplit.Length > 2)
            {
                short typePacket; short.TryParse(packetsplit[2], out typePacket);
                if (typePacket == 2)
                {
                    Session.Client.SendPacket("ishop");
                }
                else if (typePacket == 0)
                {
                    MapShop myShop = new MapShop();

                    if (packetsplit.Length > 2)
                        for (short j = 3, i = 0; j <= packetsplit.Length - 5; j += 4, i++)
                        {

                            short.TryParse(packetsplit[j], out type[i]);
                            short.TryParse(packetsplit[j + 1], out slot[i]);
                            short.TryParse(packetsplit[j + 2], out qty[i]);
                            long.TryParse(packetsplit[j + 3], out gold[i]);
                            if (qty[i] != 0)
                            {
                                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot[i], type[i]);
                                PersonalShopItem personalshopitem = new PersonalShopItem()
                                {
                                    InvSlot = slot[i],
                                    InvType = type[i],
                                    Amount = qty[i],
                                    Price = gold[i],
                                    Slot = i,
                                    Color = inv.InventoryItem.Color,
                                    Concentrate = inv.InventoryItem.Concentrate,
                                    CriticalLuckRate = inv.InventoryItem.CriticalLuckRate,
                                    CriticalRate = inv.InventoryItem.CriticalRate,
                                    DamageMaximum = inv.InventoryItem.DamageMaximum,
                                    DamageMinimum = inv.InventoryItem.DamageMinimum,
                                    DarkElement = inv.InventoryItem.DarkElement,
                                    DistanceDefence = inv.InventoryItem.DistanceDefence,
                                    DistanceDefenceDodge = inv.InventoryItem.DistanceDefenceDodge,
                                    DefenceDodge = inv.InventoryItem.DefenceDodge,
                                    ElementRate = inv.InventoryItem.ElementRate,
                                    FireElement = inv.InventoryItem.FireElement,
                                    HitRate = inv.InventoryItem.HitRate,
                                    InventoryItemId = inv.InventoryItemId,
                                    ItemVNum = inv.InventoryItem.ItemVNum,
                                    LightElement = inv.InventoryItem.LightElement,
                                    MagicDefence = inv.InventoryItem.MagicDefence,
                                    RangeDefence = inv.InventoryItem.RangeDefence,
                                    Rare = inv.InventoryItem.Rare,
                                    SpXp = inv.InventoryItem.SpXp,
                                    SpLevel = inv.InventoryItem.SpLevel,
                                    SlDefence = inv.InventoryItem.SlDefence,
                                    SlElement = inv.InventoryItem.SlElement,
                                    SlHit = inv.InventoryItem.SlHit,
                                    SlHP = inv.InventoryItem.SlHP,
                                    Upgrade = inv.InventoryItem.Upgrade,
                                    WaterElement = inv.InventoryItem.WaterElement
                                };
                                myShop.Items.Add(personalshopitem);
                            }


                        }

                    for (int i = 83; i < packetsplit.Length; i++)
                        shopname += String.Format("{0} ", packetsplit[i]);
                    shopname.TrimEnd(' ');
                    myShop.OwnerId = Session.Character.CharacterId;
                    myShop.Name = shopname;
                    Session.CurrentMap.ShopUserList.Add(Session.CurrentMap.ShopUserList.Count(), myShop);

                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePlayerFlag(Session.CurrentMap.ShopUserList.Count()), ReceiverType.AllOnMapExceptMe);

                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateShop(shopname), ReceiverType.AllOnMap);

                    Session.Client.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));
                    Session.Character.Rested = 1;
                    Session.Character.LastSpeed = Session.Character.Speed;
                    Session.Character.Speed = 0;
                    Session.Client.SendPacket(Session.Character.GenerateCond());

                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);
                    if (myShop.Items.Count == 0)
                    {
                        {
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateShopEnd(), ReceiverType.AllOnMap);
                            Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SHOP_VOID"), 10));
                        }
                    }

                }
                else if (typePacket == 1)
                {

                    KeyValuePair<long, MapShop> shop = Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId));
                    Session.CurrentMap.ShopUserList.Remove(shop.Key);

                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateShopEnd(), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePlayerFlag(0), ReceiverType.AllOnMapExceptMe);
                    Session.Character.Speed = Session.Character.LastSpeed;
                    Session.Character.Rested = 0;
                    Session.Client.SendPacket(Session.Character.GenerateCond());
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);



                }
            }

        }
        [Packet("b_i")]
        public void askToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            Session.Client.SendPacket(Session.Character.GenerateDialog(String.Format("#b_i^{0}^{1}^1 #b_i^0^0^5 {2}", type, slot, Language.Instance.GetMessageFromKey("ASK_TO_DELETE"))));
        }
        [Packet("#b_i")]
        public void answerToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);

            if (Convert.ToInt32(packetsplit[4]) == 1)
            {
                Session.Client.SendPacket(Session.Character.GenerateDialog(String.Format("#b_i^{0}^{1}^2 #b_i^0^0^5 {2}", type, slot, Language.Instance.GetMessageFromKey("SURE_TO_DELETE"))));
            }
            else if (Convert.ToInt32(packetsplit[4]) == 2)
            {
                DeleteItem(type, slot);
            }
        }
        [Packet("mve")]
        public void MoveInventory(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            short desttype; short.TryParse(packetsplit[4], out desttype);
            short destslot; short.TryParse(packetsplit[5], out destslot);
            Inventory inv = Session.Character.InventoryList.moveInventory(type, slot, desttype, destslot);
            if (inv != null)
            {
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, desttype, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Color, inv.InventoryItem.Upgrade));
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));


            }
        }

        [Packet("get")]
        public void GetItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long DropId; long.TryParse(packetsplit[4], out DropId);
            MapItem mapitem;
            if (Session.CurrentMap.DroppedList.TryGetValue(DropId, out mapitem))
            {
                short Amount = mapitem.Amount;
                if (mapitem.PositionX < Session.Character.MapX + 3 && mapitem.PositionX > Session.Character.MapX - 3 && mapitem.PositionY < Session.Character.MapY + 3 && mapitem.PositionY > Session.Character.MapY - 3)
                {
                    Session.CurrentMap.DroppedList.Remove(DropId);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                    Inventory newInv = Session.Character.InventoryList.CreateItem(mapitem, Session.Character);
                    if (newInv != null)
                    {
                        Item iteminfo = ServerManager.GetItem(newInv.InventoryItem.ItemVNum);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newInv.InventoryItem.ItemVNum, newInv.InventoryItem.Amount, newInv.Type, newInv.Slot, newInv.InventoryItem.Rare, newInv.InventoryItem.Color, newInv.InventoryItem.Upgrade));
                        Session.Client.SendPacket(Session.Character.GenerateSay(String.Format("{0}: {1} x {2}", Language.Instance.GetMessageFromKey("YOU_GET_OBJECT"), iteminfo.Name, Amount), 12));
                    }
                }



            }
        }

        [Packet("#req_exc")]
        public void AcceptExchange(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            short mode; short.TryParse(packetsplit[2], out mode);
            long charId; long.TryParse(packetsplit[3], out charId);
            Session.Character.ExchangeInfo = new ExchangeInfo();
            Session.Character.ExchangeInfo.CharId = charId;
            Session.Character.ExchangeInfo.Confirm = false;
            if (mode == 2)
            {
                Session.Client.SendPacket(String.Format("exc_list 1 {0} -1", charId));
                ClientLinkManager.Instance.Broadcast(Session, String.Format("exc_list 1 {0} -1", Session.Character.CharacterId), ReceiverType.OnlySomeone, "", charId);
            }
            if (mode == 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateModal("refused", 0));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateModal("refused", 0), ReceiverType.OnlySomeone, "", charId);
            }

        }
        [Packet("exc_list")]
        public void ExchangeList(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long Gold = 0; long.TryParse(packetsplit[2], out Gold);
            short[] type = new short[10];
            short[] slot = new short[10];
            short[] qty = new short[10];
            string packetList = "";
            for (int j = 6, i = 0; j <= packetsplit.Length; j += 3, i++)
            {
                short.TryParse(packetsplit[j - 3], out type[i]);
                short.TryParse(packetsplit[j - 2], out slot[i]);
                short.TryParse(packetsplit[j - 1], out qty[i]);
                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot[i], type[i]);
                InventoryItem item = inv.InventoryItem;
                Session.Character.ExchangeInfo.ExchangeList.Add(item);
                item.Amount = qty[i];
                packetList += String.Format("{0}.{1}.{2}.{3} ", i, type[i], item.ItemVNum, qty[i]);
            }
            Session.Character.ExchangeInfo.Gold = Gold;
            ClientLinkManager.Instance.Broadcast(Session, String.Format("exc_list 1 {0} {1} {2}", Session.Character.CharacterId, Gold, packetList), ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);
            Session.Character.ExchangeInfo.Validate = true;
        }
        [Packet("req_exc")]
        public void Exchange(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short mode; short.TryParse(packetsplit[2], out mode);

            long charId = -1;

            string CharName;
            if (mode == 1)
            {
                long.TryParse(packetsplit[3], out charId);
                Session.Character.ExchangeInfo = new ExchangeInfo();
                Session.Character.ExchangeInfo.CharId = charId;
                CharName = (string)ClientLinkManager.Instance.RequiereProperties(charId, "Name");
                Session.Client.SendPacket(Session.Character.GenerateModal(String.Format("{0}{1}", Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE"), "", charId), 0));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateDialog(String.Format("#req_exc^2^{0} #req_exc^5^{0} {1}", Session.Character.CharacterId, "accept?")), ReceiverType.OnlySomeone, CharName);
                Session.Character.ExchangeInfo.Confirm = false;

            }
            if (mode == 4)
            {

                Session.Client.SendPacket("exc_close 0");
                ClientLinkManager.Instance.Broadcast(Session, String.Format("exc_close 0"), ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

            }
            if (mode == 3)
            {
                ExchangeInfo exchange = (ExchangeInfo)ClientLinkManager.Instance.RequiereProperties(Session.Character.ExchangeInfo.CharId, "ExchangeInfo");
                int backpack = (int)ClientLinkManager.Instance.RequiereProperties(Session.Character.ExchangeInfo.CharId, "BackPack");
                InventoryList inventory = (InventoryList)ClientLinkManager.Instance.RequiereProperties(Session.Character.ExchangeInfo.CharId, "InventoryList");
                if (Session.Character.ExchangeInfo.Validate && exchange.Validate)
                {
                    Session.Character.ExchangeInfo.Confirm = true;
                    if (exchange.Confirm)
                    {
                        Session.Client.SendPacket("exc_close 1");
                        ClientLinkManager.Instance.Broadcast(Session, String.Format("exc_close 1"), ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);
                        bool continu = true;

                        if (!Session.Character.InventoryList.getFreePlaceAmount(Session.Character.ExchangeInfo.ExchangeList, Session.Character.BackPack))
                        {
                            continu = false;
                        }

                        if (!inventory.getFreePlaceAmount(exchange.ExchangeList, backpack))
                        {
                            continu = false;
                        }
                        foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                        {
                            Inventory inv = Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                            if (inv != null && ServerManager.GetItem(inv.InventoryItem.ItemVNum).Transaction != 1)
                                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_TRADABLE"), 0));
                            continu = false;
                            break;
                        }
                        if (continu == false)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0), ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            Session.Client.SendPacket("exc_close 0");
                            ClientLinkManager.Instance.Broadcast(Session, String.Format("exc_close 0"), ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);
                        }
                        else
                        {
                            foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                            {
                                Inventory inv = Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                                Session.Character.InventoryList.DeleteByInventoryItemId(item.InventoryItemId);
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));

                            }
                            foreach (InventoryItem item in exchange.ExchangeList)
                            {
                                Inventory inv = Session.Character.InventoryList.CreateItem(item, Session.Character);
                                if (inv != null)
                                {

                                    short Slot = inv.Slot;
                                    if (Slot != -1)
                                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, Slot, inv.InventoryItem.Rare, inv.InventoryItem.Color, inv.InventoryItem.Upgrade));
                                }
                            }

                            Session.Character.Gold = Session.Character.Gold - Session.Character.ExchangeInfo.Gold + exchange.Gold;
                            Session.Client.SendPacket(Session.Character.GenerateGold());
                            ClientLinkManager.Instance.ExchangeValidate(Session, Session.Character.ExchangeInfo.CharId);

                        }


                    }
                    else
                    {
                        CharName = (string)ClientLinkManager.Instance.RequiereProperties(charId, "Name");

                        Session.Client.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("IN_WAITING_FOR"), CharName)));
                    }
                }
            }
        }
        [Packet("mvi")]
        public void MoveItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            short amount; short.TryParse(packetsplit[4], out amount);
            short destslot; short.TryParse(packetsplit[5], out destslot);
            Inventory LastInventory;
            Inventory NewInventory;
            Session.Character.InventoryList.MoveItem(Session.Character, type, slot, amount, destslot, out LastInventory, out NewInventory);
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(NewInventory.InventoryItem.ItemVNum, NewInventory.InventoryItem.Amount, type, NewInventory.Slot, NewInventory.InventoryItem.Rare, NewInventory.InventoryItem.Color, NewInventory.InventoryItem.Upgrade));
            if (LastInventory != null)
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(LastInventory.InventoryItem.ItemVNum, LastInventory.InventoryItem.Amount, type, LastInventory.Slot, LastInventory.InventoryItem.Rare, LastInventory.InventoryItem.Color, LastInventory.InventoryItem.Upgrade));
            else
            {
                DeleteItem(type, slot);
            }

        }
        [Packet("req_info")]
        public void ReqInfo(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            ClientLinkManager.Instance.RequiereBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateReqInfo");
        }

        [Packet("u_i")]
        public void UseItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 8)
            {
                short uitype; short.TryParse(packetsplit[2], out uitype);
                short type; short.TryParse(packetsplit[4], out type);
                short slot; short.TryParse(packetsplit[5], out slot);
                switch (uitype)
                {
                    case 1:
                        Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        InventoryItem item = inv.InventoryItem;
                        Item itemInfo = ServerManager.GetItem(item.ItemVNum);
                        if (itemInfo.isConsumable)
                            item.Amount--;
                        if (itemInfo.Morph != 0)
                        {
                            if (!Session.Character.IsVehicled)
                            {
                                if (Session.Character.ThreadCharChange != null && Session.Character.ThreadCharChange.IsAlive)
                                    Session.Character.ThreadCharChange.Abort();
                                Session.Character.ThreadCharChange = new Thread(() => ChangeVehicle(itemInfo));
                                Session.Character.ThreadCharChange.Start();
                            }
                            else
                            {
                                RemoveVehicle();
                            }
                        }

                        break;

                }
            }
        }

        [Packet("eqinfo")]
        public void EqInfo(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                short type = 0; short.TryParse(packetsplit[3], out type);
                short slot = 0; short.TryParse(packetsplit[2], out slot);

                Inventory inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);

                if (inventory != null)
                {
                    Session.Client.SendPacket(Session.Character.GenerateEInfo(inventory.InventoryItem));
                }
            }
        }
        [Packet("sl")]
        public void SpTransform(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment);
            if (sp != null)
            {
                if (!Session.Character.UseSp)
                {
                    double def = (((TimeSpan)(DateTime.Now - new DateTime(2010, 1, 1, 0, 0, 0))).TotalSeconds) - (Session.Character.LastSp);
                    if (def >= 30)
                    {
                        if (Session.Character.ThreadCharChange != null && Session.Character.ThreadCharChange.IsAlive)
                            Session.Character.ThreadCharChange.Abort();
                        Session.Character.ThreadCharChange = new Thread(() => ChangeSP());
                        Session.Character.ThreadCharChange.Start();


                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), 30 - (int)def), 0));
                    }
                }
                else
                {
                    Session.Character.LastSp = (((TimeSpan)(DateTime.Now - new DateTime(2010, 1, 1, 0, 0, 0))).TotalSeconds);
                    Thread removeSP = new Thread(() => RemoveSP(sp.InventoryItem.ItemVNum));
                    removeSP.Start();
                }
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
            }
        }
        [Packet("/")]
        public void Whisper(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";
            message.Trim();

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlyMe);
            if (!ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlySomeone, packetsplit[1].Substring(1)))
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")), ReceiverType.OnlyMe);

        }
        [Packet("hero")]
        public void Hero(string packet)
        {

            if (DAOFactory.CharacterDAO.IsReputHero(Session.Character.CharacterId) >= 3)
            {
                string[] packetsplit = packet.Split(' ');
                string message = String.Empty;
                for (int i = 2; i < packetsplit.Length; i++)
                    message += packetsplit[i] + " ";
                message.Trim();

                ClientLinkManager.Instance.Broadcast(Session, String.Format("msg 5 [{0}]:{1}", Session.Character.Name, message), ReceiverType.All);
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
        }
        [Packet("compl")]
        public void Compliment(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long complimentCharacterId = 0;
            if (long.TryParse(packetsplit[3], out complimentCharacterId))
            {
                if (Session.Character.Level >= 30)
                {
                    if (Session.Account.LastLogin.AddMinutes(60) <= DateTime.Now)
                    {
                        if (Session.Account.LastCompliment.Date.AddDays(1) <= DateTime.Now.Date)
                        {
                            CharacterDTO complimentCharacter = DAOFactory.CharacterDAO.LoadById(complimentCharacterId);
                            complimentCharacter.Compliment++;
                            DAOFactory.CharacterDAO.InsertOrUpdate(ref complimentCharacter);
                            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"), complimentCharacter.Name), 12));
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"), Session.Character.Name), 12), ReceiverType.OnlySomeone, packetsplit[1].Substring(1));
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                        }
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"), (Session.Account.LastLogin - DateTime.Now).Minutes), 11));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"), 11));
                }
            }
        }


        #endregion

        #region AdminCommand
        [Packet("$Command")]
        public void Command(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay("-----------Commands Info--------------", 10));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport CharacterName", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeSex", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Kick USERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$MapDance", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic MUSIC", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Invisible", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Position", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shutdown", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));

        }
        [Packet("$CreateItem")]
        public void CreateItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short amount = 1;
            short vnum, rare = 0, upgrade = 0, color = 0, level = 0;
            ItemDTO iteminfo = null;
            if (packetsplit.Length != 5 && packetsplit.Length != 4)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 10));
            }
            else if (Int16.TryParse(packetsplit[2], out vnum))
            {
                iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.Colored)
                    {
                        Int16.TryParse(packetsplit[3], out color);
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (packetsplit.Length == 5)
                        {
                            if (iteminfo.EquipmentSlot == Convert.ToByte((short)EquipmentType.Sp))
                            {
                                Int16.TryParse(packetsplit[3], out upgrade);
                                Int16.TryParse(packetsplit[4], out color);
                                level = 1;
                            }
                            else
                            {
                                Int16.TryParse(packetsplit[3], out rare);
                                Int16.TryParse(packetsplit[4], out upgrade);
                            }

                        }
                    }
                    else
                    {
                        Int16.TryParse(packetsplit[3], out amount);
                    }
                    InventoryItem newItem = new InventoryItem()
                    {
                        InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                        Amount = amount,
                        ItemVNum = vnum,
                        Rare = rare,
                        Upgrade = upgrade,
                        Color = color,
                        Concentrate = 0,
                        CriticalLuckRate = 0,
                        CriticalRate = 0,
                        DamageMaximum = 0,
                        DamageMinimum = 0,
                        DarkElement = 0,
                        DistanceDefence = 0,
                        DistanceDefenceDodge = 0,
                        DefenceDodge = 0,
                        ElementRate = 0,
                        FireElement = 0,
                        HitRate = 0,
                        LightElement = 0,
                        MagicDefence = 0,
                        RangeDefence = 0,
                        SpXp = 0,
                        SpLevel = level,
                        SlDefence = 0,
                        SlElement = 0,
                        SlHit = 0,
                        SlHP = 0,
                        WaterElement = 0,

                    };
                    Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                    if (inv != null)
                    {
                        short Slot = inv.Slot;
                        if (Slot != -1)
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(vnum, inv.InventoryItem.Amount, iteminfo.Type, Slot, rare, color, upgrade));
                    }
                }
            }
        }
        [Packet("$Position")]
        public void Position(string packet)
        {

            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format("Map:{0} - X:{1} - Y:{2}", Session.Character.MapId, Session.Character.MapX, Session.Character.MapY), 12));

        }
        [Packet("$Kick")]
        public void Kick(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
                ClientLinkManager.Instance.Kick(packetsplit[2]);
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Kick USERNAME", 10));

        }
        [Packet("$ChangeClass")]
        public void ChangeClass(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte classe;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out classe) && classe < 4)
                {
                    ClassChange(classe);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 10));
        }
        [Packet("$ChangeRep")]
        public void Rep(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            long reput;
            if (packetsplit.Length > 3)
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 10));
            if (Int64.TryParse(packetsplit[2], out reput) && reput > 0)
            {

                Session.Character.Reput = reput;
                Session.Client.SendPacket(Session.Character.GenerateFd());
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            }
        }
        [Packet("$Lvl")]
        public void Lvl(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            byte level;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out level) && level < 100 && level > 0)
                {

                    Session.Character.Level = level;
                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                    Session.Client.SendPacket(Session.Character.GenerateStat());
                    //sc 0 0 31 39 31 4 70 1 0 33 35 43 2 70 0 17 35 19 35 17 0 0 0 0
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 10));
        }
        [Packet("$JLvl")]
        public void JLvl(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte joblevel;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out joblevel) && ((Session.Character.Class == 0 && joblevel <= 20) || (Session.Character.Class != 0 && joblevel <= 80)) && joblevel > 0)
                {
                    Session.Character.JobLevel = joblevel;
                    Session.Client.SendPacket(Session.Character.GenerateLev());

                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$JLvl JLVL", 10));
        }
        [Packet("$SPLvl")]
        public void SPLvl(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte splevel;
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment);
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out splevel) && splevel <= 99 && splevel > 0)
                {
                    sp.InventoryItem.SpLevel = splevel;
                    Session.Client.SendPacket(Session.Character.GenerateLev());

                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                }
            }
            else

                Session.Client.SendPacket(Session.Character.GenerateSay("$SPLvl SPLVL", 10));
        }
        [Packet("$ChangeSex")]
        public void Gender(string packet)
        {
            Session.Character.Gender = Session.Character.Gender == 1 ? (byte)0 : (byte)1;
            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.OnlyMe);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGender(), ReceiverType.OnlyMe);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
        }
        [Packet("$Ban")]
        public void Ban(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                ClientLinkManager.Instance.Kick(packetsplit[2]);
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                    DAOFactory.AccountDAO.ToggleBan(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 10));


        }
        [Packet("$Shutdown")]
        public void Shutdown(string packet)
        {
            if (ClientLinkManager.Instance.shutdownActive == false)
            {
                Thread ThreadShutdown = new Thread(new ThreadStart(ShutdownThread));
                ThreadShutdown.Start();
                ClientLinkManager.Instance.shutdownActive = true;
            }

        }

        [Packet("$Shout")]
        public void Shout(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            if (packetsplit.Length > 2)
                for (int i = 2; i < packetsplit.Length; i++)
                    message += packetsplit[i] + " ";
            message.Trim();

            ClientLinkManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);

        }

        [Packet("$MapDance")]
        public void MapDance(string packet)
        {
            Session.CurrentMap.IsDancing = Session.CurrentMap.IsDancing == 0 ? 2 : 0;
            if (Session.CurrentMap.IsDancing == 2)
            {
                Session.Character.Dance();
                ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "Dance");
                ClientLinkManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 2");
            }
            else
            {
                Session.Character.Dance();
                ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "Dance");
                ClientLinkManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance");
            }


        }
        [Packet("$Invisible")]
        public void Invisible(string packet)
        {
            Session.Character.Invisible = Session.Character.Invisible == 0 ? 1 : 0;
            ChangeMap();

        }

        [Packet("$Effect")]
        public void Effect(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = 0;
            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(arg), ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECT", 10));

        }
        [Packet("$Resize")]
        public void Resize(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = -1;

            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);

                if (arg > -1)

                {
                    Session.Character.Size = arg;
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateScal(), ReceiverType.AllOnMap);

                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 10));

        }
        [Packet("$PlayMusic")]
        public void PlayMusic(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = -1;
            if (packetsplit.Length > 2)
            {
                if (packetsplit.Length > 1)
                {
                    short.TryParse(packetsplit[2], out arg);
                    if (arg > -1)
                        ClientLinkManager.Instance.Broadcast(Session, String.Format("bgm {0}", arg), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic BGMUSIC", 10));
        }
        [Packet("$Morph")]
        public void Morph(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[4];
            bool verify = false;
            if (packetsplit.Length > 5)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && short.TryParse(packetsplit[5], out arg[3]));
            }
            switch (packetsplit.Length)
            {


                case 6:
                    if (verify)
                    {
                        Session.Character.UseSp = true;
                        Session.Character.Morph = arg[0];
                        Session.Character.MorphUpgrade = arg[1];
                        Session.Character.MorphUpgrade2 = arg[2];
                        Session.Character.ArenaWinner = arg[3];
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);

                    }
                    break;
                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
                    break;
            }

        }
        [Packet("$Teleport")]
        public void Teleport(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[3];
            bool verify = false;

            if (packetsplit.Length > 4)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && DAOFactory.MapDAO.LoadById(arg[0]) != null);
            }
            switch (packetsplit.Length)
            {

                case 3:
                    string name = packetsplit[2];
                    object mapy = ClientLinkManager.Instance.RequiereProperties(name, "MapY");
                    object mapx = ClientLinkManager.Instance.RequiereProperties(name, "MapX");
                    object mapId = ClientLinkManager.Instance.RequiereProperties(name, "MapId");
                    if (String.Format("{0}", mapy) != "" && String.Format("{0}", mapx) != "" && String.Format("{0}", mapId) != "")
                    {
                        Session.Character.MapId = (short)mapId;
                        Session.Character.MapX = (short)((short)(mapx) + (short)1);
                        Session.Character.MapY = (short)((short)(mapy) + (short)1);
                        MapOut();

                        ChangeMap();
                    }
                    break;
                case 5:
                    if (verify)
                    {
                        Session.Character.MapId = arg[0];
                        Session.Character.MapX = arg[1];
                        Session.Character.MapY = arg[2];
                        MapOut();
                        ChangeMap();
                    }
                    break;
                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                    break;
            }

        }

        [Packet("$Speed")]
        public void Speed(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            int arg = 0;
            bool verify = false;
            if (packetsplit.Length > 2)
            {
                verify = (int.TryParse(packetsplit[2], out arg));
            }
            switch (packetsplit.Length)
            {


                case 3:
                    if (verify)
                    {
                        Session.Character.Speed = arg;
                    }
                    break;
                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 10));
                    break;
            }

        }
        [Packet("$Rarify")]
        public void Rarify(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 10));
            }
            else
            {

                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);


                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    InventoryDTO inventoryDTO = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, itemslot, 0);
                    InventoryItemDTO item = DAOFactory.InventoryItemDAO.LoadById(inventoryDTO.InventoryItemId);
                    int result = RarifyItem(item, (InventoryItem.RarifyMode)mode, (InventoryItem.RarifyProtection)protection);
                    if (result == -1)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 11));
                    }
                    else if (result == 0)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 11));
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), result), 12));
                    }
                }
            }
        }
        [Packet("$Upgrade")]
        public void Upgrade(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 10));
            }
            else
            {

                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);


                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    InventoryDTO inventoryDTO = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, itemslot, 0);
                    InventoryItemDTO item = DAOFactory.InventoryItemDAO.LoadById(inventoryDTO.InventoryItemId);
                    int result = UpgradeItem(item, (InventoryItem.UpgradeMode)mode, (InventoryItem.UpgradeProtection)protection);
                    if (result == -1)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                    }
                    else if (result == 0)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 11));
                    }
                    else if (result == 1)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                    }
                }
            }
        }
        #endregion

        #region Methods
        public void MapOut()
        {
            Session.Client.SendPacket(Session.Character.GenerateMapOut());
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
        }
        public void ChangeMap()
        {
            Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);
            Session.Client.SendPacket(Session.Character.GenerateCInfo());
            Session.Client.SendPacket(Session.Character.GenerateCMode());
            Session.Client.SendPacket(Session.Character.GenerateFaction());
            Session.Client.SendPacket(Session.Character.GenerateFd());
            Session.Client.SendPacket(Session.Character.GenerateLev());
            Session.Client.SendPacket(Session.Character.GenerateStat());
            //ski
            Session.Client.SendPacket(Session.Character.GenerateAt());
            Session.Client.SendPacket(Session.Character.GenerateCMap());
            if (Session.Character.Size != 10)
                Session.Client.SendPacket(Session.Character.GenerateScal());
            foreach (String portalPacket in Session.Character.GenerateGp())
                Session.Client.SendPacket(portalPacket);
            foreach (String npcPacket in Session.Character.Generatein2())
                Session.Client.SendPacket(npcPacket);
            foreach (String ShopPacket in Session.Character.GenerateNPCShopOnMap())
                Session.Client.SendPacket(ShopPacket);
            foreach (String droppedPacket in Session.Character.GenerateDroppedItem())
                Session.Client.SendPacket(droppedPacket);

            //sc
            Session.Client.SendPacket(Session.Character.GenerateCond());
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
            Session.Client.SendPacket(String.Format("rsfi {0} {1} {2} {3} {4} {5}", 1, 1, 4, 9, 4, 9));//stone act
            ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "GenerateIn");
            //need to see if changeMap change the sp :D  ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "GenerateCMode");

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            if (Session.CurrentMap.IsDancing == 2 && Session.Character.IsDancing == 0)
                ClientLinkManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 2");
            else if (Session.CurrentMap.IsDancing == 0 && Session.Character.IsDancing == 1)
            {
                Session.Character.IsDancing = 0;
                ClientLinkManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance");

            }
            foreach (String ShopPacket in Session.Character.GenerateShopOnMap())
                Session.Client.SendPacket(ShopPacket);

            foreach (String ShopPacketChar in Session.Character.GeneratePlayerShopOnMap())
                Session.Client.SendPacket(ShopPacketChar);

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
            Session.Client.SendPacket(Session.Character.GenerateEquipment());
            GenerateRankings();
        }
        public void healthThread()
        {
            int x = 1;
            while (true)
            {
                bool change = false;
                if (Session.Character.Rested == 1)
                    Thread.Sleep(1500);
                else
                    Thread.Sleep(2000);
                if (x == 0)
                    x = 1;

                if (Session.Character.Hp + Session.Character.HealthHPLoad() < Session.Character.HPLoad())
                {
                    change = true;
                    Session.Character.Hp += Session.Character.HealthHPLoad();
                }

                else
                    Session.Character.Hp = (int)Session.Character.HPLoad();

                if (x == 1)
                {
                    if (Session.Character.Mp + Session.Character.HealthMPLoad() < Session.Character.MPLoad())
                    {
                        Session.Character.Mp += Session.Character.HealthMPLoad();
                        change = true;
                    }
                    else
                        Session.Character.Mp = (int)Session.Character.MPLoad();
                    x = 0;
                }
                if (change)
                {
                    ClientLinkManager.Instance.Broadcast(Session,
         Session.Character.GenerateStat(),
           ReceiverType.AllOnMap);
                }


            }
        }
        public void loadShopItem(long owner, KeyValuePair<long, MapShop> shop)
        {

            string packetToSend = String.Format("n_inv 1 {0} 0 0", owner);
            for (short i = 0; i < 20; i++)
            {
                PersonalShopItem item = shop.Value.Items.FirstOrDefault(it => it.Slot.Equals(i));
                if (item != null)
                {
                    if (ServerManager.GetItem(item.ItemVNum).Type == 0)
                        packetToSend += String.Format(" {0}.{1}.{2}.{3}.{4}.{5}.", 0, i, ServerManager.GetItem(item.ItemVNum).VNum, item.Rare, item.Upgrade, item.Price);
                    else
                        packetToSend += String.Format(" {0}.{1}.{2}.{3}.{4}.{5}.", ServerManager.GetItem(item.ItemVNum).Type, i, ServerManager.GetItem(item.ItemVNum).VNum, item.Amount, item.Price, -1);
                }
                else
                {
                    packetToSend += " -1";
                }
            }
            packetToSend += " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1";

            Session.Client.SendPacket(packetToSend);
        }
        public void RemoveSP(short vnum)
        {
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment);

            Session.Character.Speed -= ServerManager.GetItem(vnum).Speed;
            Session.Character.UseSp = false;

            // string s2 = "c_info " + chara.name + " - -1 -1 - " + chara.id + " " + ((chara.isGm) ? 2 : 0) + " " + +chara.sex + " " + +chara.Hair.style + " " + +chara.Hair.color + " " + chara.user_class + " " + Stats.GetReput(chara.Reput, chara.dignite.ToString()) + " " + (chara.Sp.inUsing ? chara.Sp.sprite : 0) + " 0 - " + (chara.Sp.inUsing ? chara.Sp.upgrade == 15 ? chara.Sp.wings > 4 ? 0 : 15 : chara.Sp.upgrade : 0) + " " + (chara.Sp.inUsing ? (chara.Sp.wings > 4) ? chara.Sp.wings - 4 : chara.Sp.wings : 0) + " " + (chara.Sp.wings_arena ? 1 : 0);
            //chara.Send(s2);
            //s2 = "at " + chara.id + " " + chara.MapPoint.map + " " + chara.MapPoint.x + " " + +chara.MapPoint.y + " 2 0 0 1";
            //chara.Send(s2);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCond(), ReceiverType.AllOnMap);

            Session.Client.SendPacket(Session.Character.GenerateLev());
            /*string s="sl 0";
               chara.Send(s);*/
            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("STAY_TIME")), 11));
            Session.Client.SendPacket("sd 30");

            /*

            s = "cond 1 " + chara.id + " 0 0 12";
            chara.Send(s);
            */
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);

            ClientLinkManager.Instance.Broadcast(Session, String.Format("guri 6 1 {0} 0 0", Session.Character.CharacterId), ReceiverType.AllOnMap);

            /*
                    s="ms_c";
                        chara.Send(s);
                        */

            //  lev 40 2288403 23 47450 3221180 113500 20086 5
            Thread.Sleep(30000);
            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPEAR")), 11));
            Session.Client.SendPacket("sd 0");

        }
        public void ChangeSP()
        {

            Session.Client.SendPacket("delay 5000 3 #sl^1");
            ClientLinkManager.Instance.Broadcast(Session, String.Format("guri 2 1 {0}", Session.Character.CharacterId), ReceiverType.AllOnMap);
            Thread.Sleep(5000);
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment);
            Inventory fairy = Session.Character.EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);

            if (Session.Character.Reput >= ServerManager.GetItem(sp.InventoryItem.ItemVNum).ReputationMinimum)
            {
                if (!(fairy != null && ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element != ServerManager.GetItem(sp.InventoryItem.ItemVNum).Element))
                {


                    Session.Character.UseSp = true;
                    Session.Character.Morph = ServerManager.GetItem(sp.InventoryItem.ItemVNum).Morph;
                    Session.Character.MorphUpgrade = sp.InventoryItem.Upgrade;
                    Session.Character.MorphUpgrade2 = sp.InventoryItem.Color;
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);


                    /*s = "ski 833 833 833 834 835 836 837 838 839 840 841 21 25 28 37 41 44 49 53 56 340 341 345 352";
                    MainFile.maps.SendMap(chara, s, true);
                    /*
                     qslot 0 1.1.2 1.1.1 1.1.3 0.7.-1 1.1.0 0.7.-1 0.7.-1 0.1.10 1.3.2 1.3.1

                     qslot 1 1.1.2 1.1.3 1.1.4 1.1.5 1.1.6 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1

                     qslot 2 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1
                     */



                    //  lev 40 2288403 14 72745 3221180 145000 20086 5

                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);

                    ClientLinkManager.Instance.Broadcast(Session, String.Format("guri 6 1 {0} 0 0", Session.Character.CharacterId), ReceiverType.AllOnMap);
                    Session.Client.SendPacket(Session.Character.GenerateSpPoint());
                    Session.Character.Speed += ServerManager.GetItem(sp.InventoryItem.ItemVNum).Speed;
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCond(), ReceiverType.AllOnMap);
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                }
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"), 0));
            }

        }
        public void GenerateRankings()
        {
            string clinit = "clinit";
            string flinit = "flinit";
            string kdlinit = "kdlinit";

            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopComplimented())
            {
                clinit += String.Format(" {0}|{1}|{2}|{3}", character.CharacterId, character.Level, character.Compliment, character.Name);
            }
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopReputation())
            {
                flinit += String.Format(" {0}|{1}|{2}|{3}", character.CharacterId, character.Level, character.Reput, character.Name);
            }
            if (false)/*Need to delete it when gettoppoint will not return null*/
                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopPoints())
                {
                    kdlinit += String.Format(" {0}|{1}|{2}|{3}", character.CharacterId, character.Level, 0/*replace with true var*/, character.Name);
                }

            Session.Client.SendPacket(clinit);
            Session.Client.SendPacket(flinit);
            Session.Client.SendPacket(kdlinit);
        }
        public void ChangeVehicle(Item item)
        {
            Session.Client.SendPacket("delay 2500 3 #sl^1");
            ClientLinkManager.Instance.Broadcast(Session, String.Format("guri 2 1 {0}", Session.Character.CharacterId), ReceiverType.AllOnMap);

            Thread.Sleep(2500);
            Session.Character.IsVehicled = true;
            Session.Character.Speed = item.Speed;
            Session.Character.Morph = item.Morph + Session.Character.Gender;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, String.Format("guri 6 1 {0} 0 0", Session.Character.CharacterId), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);
        }
        public void RemoveVehicle()
        {
            Session.Character.IsVehicled = false;
            Session.Character.Speed = ServersData.SpeedData[Session.Character.Class];
            if (Session.Character.UseSp)
            {
                Session.Character.Morph = ServerManager.GetItem(Session.Character.EquipmentList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment).InventoryItem.ItemVNum).Morph;
            }
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, String.Format("guri 6 1 {0} 0 0", Session.Character.CharacterId), ReceiverType.AllOnMap);

        }
        public void ShutdownThread()
        {
            string message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            ClientLinkManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            Thread.Sleep(60000 * 4);
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            ClientLinkManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            Thread.Sleep(30000);
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            ClientLinkManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            Thread.Sleep(30000);
            //save
            Environment.Exit(0);
        }
        public void DeleteItem(short type, short slot)
        {
            Session.Character.InventoryList.DeleteFromSlotAndType(slot, type);
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));

        }
        public void ClassChange(byte classe)
        {
            Session.Client.SendPacket("npinfo 0");
            Session.Client.SendPacket("p_clear");

            Session.Character.Class = classe;
            Session.Character.Speed = ServersData.SpeedData[Session.Character.Class];
            Session.Character.Hp = (int)Session.Character.HPLoad();
            Session.Character.Mp = (int)Session.Character.MPLoad();
            Session.Client.SendPacket(Session.Character.GenerateTit());

            // eq 37 0 1 0 9 3 -1.120.46.86.-1.-1.-1.-1 0 0
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);

            //equip 0 0 0.46.0.0.0 1.120.0.0.0 5.86.0.0.0

            Session.Client.SendPacket(Session.Character.GenerateLev());
            Session.Client.SendPacket(Session.Character.GenerateStat());
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(8), ReceiverType.AllOnMap);
            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOB_CHANGED"), 0));
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);
            Random rand = new Random();
            int faction = 1 + (int)rand.Next(0, 2);
            Session.Character.Faction = faction;
            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(String.Format("GET_PROTECTION_POWER_{0}", faction)), 0));
            Session.Client.SendPacket("scr 0 0 0 0 0 0");

            Session.Client.SendPacket(Session.Character.GenerateFaction());
            this.GetStats(String.Empty);

            Session.Client.SendPacket(Session.Character.GenerateEff(4799 + faction));
        }
        public int RarifyItem(InventoryItemDTO item, InventoryItem.RarifyMode mode, InventoryItem.RarifyProtection protection)
        {
            double rare1 = 50;
            double rare2 = 35;
            double rare3 = 25;
            double rare4 = 10;
            double rare5 = 10;
            double rare6 = 5;
            double rare7 = 1;
            double reducedchancefactor = 1.1;

            short itempricevnum = 0;
            short goldprice = 500;
            double reducedpricefactor = 0.5;

            if (protection == InventoryItem.RarifyProtection.RedAmulet)
            {
                rare1 = rare1 * reducedchancefactor;
                rare2 = rare2 * reducedchancefactor;
                rare3 = rare3 * reducedchancefactor;
                rare4 = rare4 * reducedchancefactor;
                rare5 = rare5 * reducedchancefactor;
                rare6 = rare6 * reducedchancefactor;
                rare7 = rare7 * reducedchancefactor;
            }
            switch (mode)
            {
                case InventoryItem.RarifyMode.Free:
                    break;
                case InventoryItem.RarifyMode.Reduced:
                    //TODO: Reduced Item Amount
                    Session.Character.Gold = Session.Character.Gold - (long)(goldprice * reducedpricefactor);
                    break;
                case InventoryItem.RarifyMode.Normal:
                    //TODO: Normal Item Amount
                    Session.Character.Gold = Session.Character.Gold - goldprice;
                    break;
            }

            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= rare7)
            {
                return item.Rare = 7;
            }
            else if (rnd <= rare6)
            {
                return item.Rare = 6;
            }
            else if (rnd <= rare5)
            {
                return item.Rare = 5;
            }
            else if (rnd <= rare4)
            {
                return item.Rare = 4;
            }
            else if (rnd <= rare3)
            {
                return item.Rare = 3;
            }
            else if (rnd <= rare2)
            {
                return item.Rare = 2;
            }
            else if (rnd <= rare1)
            {
                return item.Rare = 1;
            }
            else
            {
                if (protection == InventoryItem.RarifyProtection.None)
                {
                    //TODO: Item Destroy
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int UpgradeItem(InventoryItemDTO item, InventoryItem.UpgradeMode mode, InventoryItem.UpgradeProtection protection)
        {
            #region Upgrade Stats
            short[] upsuccess = { 100, 100, 90, 80, 60, 40, 20, 10, 5, 1 };
            short[] upfix = { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };
            #endregion
            short itempricevnum1 = 0;
            short itempricevnum2 = 0;
            short goldprice = 500;
            double reducedpricefactor = 0.5;

            switch (mode)
            {
                case InventoryItem.UpgradeMode.Free:
                    break;
                case InventoryItem.UpgradeMode.Reduced:
                    //TODO: Reduced Item Amount
                    Session.Character.Gold = Session.Character.Gold - (long)(goldprice * reducedpricefactor);
                    break;
                case InventoryItem.UpgradeMode.Normal:
                    //TODO: Normal Item Amount
                    Session.Character.Gold = Session.Character.Gold - goldprice;
                    break;
            }

            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= upsuccess[item.Upgrade + 1])
            {
                item.Upgrade++;
                return 2;
            }
            else if (rnd <= upfix[item.Upgrade + 1])
            {

                return 1;
            }
            else
            {
                if (protection == InventoryItem.UpgradeProtection.None)
                {
                    //TODO: Item Destroy
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion

        #region UselessPacket
        [Packet("snap")]
        public void Snap(string packet)
        {
            //i don't need this for the moment
        }

        [Packet("lbs")]
        public void Lbs(string packet)
        {
            //i don't know why there is this packet
        }
        [Packet("#sl")]
        public void sl(string packet)
        {
            //i don't know why there is this packet
        }

        [Packet("c_close")]
        public void CClose(string packet)
        {
            //i don't know why there is this packet
        }

        [Packet("f_stash_end")]
        public void FStashEnd(string packet)
        {
            //i don't know why there is this packet
        }

        #endregion
    }
}
