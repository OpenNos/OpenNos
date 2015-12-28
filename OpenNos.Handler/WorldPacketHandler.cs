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

namespace OpenNos.Handler
{
    public class WorldPacketHandler
    {
        private readonly ClientSession _session;
        public ClientSession Session
        {
            get { return _session; }
        }

        public WorldPacketHandler(ClientSession session)
        {
            _session = session;
        }

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
                        StateEnum = CharacterState.Active
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
                //ServiceFactory.Instance.CommunicationService.HasRegisteredPlayerLogin(loginPacketParts, )



                bool value = true;
                try
                { value = ServiceFactory.Instance.CommunicationService.HasRegisteredPlayerLogin(loginPacketParts[4], Session.SessionId); }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                }
                if (loginPacketParts.Length > 4 && value)
                    {

                        AccountDTO accountDTO = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);

                        if (accountDTO != null)
                        {
                            if (accountDTO.Password.Equals(EncryptionBase.sha256(loginPacketParts[6]))
                                && accountDTO.LastSession.Equals(Session.SessionId))
                            {
                                Session.Account = new GameObject.Account()
                                {
                                    AccountId = accountDTO.AccountId,
                                    Name = accountDTO.Name,
                                    Password = accountDTO.Password,
                                    Authority = accountDTO.Authority
                                };
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
                
            }

            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
            Logger.Log.InfoFormat(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), Session.SessionId);
            Session.Client.SendPacket("clist_start 0");
            foreach (CharacterDTO character in characters)
            {
                //move to character
                Session.Client.SendPacket(String.Format("clist {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}.{10}.{11}.{12}.{13}.{14}.{15}.{16} {17} {18} {19} {20}.{21} {22} {23}",
                    character.Slot, character.Name, 0, character.Gender, character.HairStyle, character.HairColor, 5, character.Class, character.Level, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -1, -1, character.HairColor, 0));
            }
            Session.Client.SendPacket("clist_end");

        }

        [Packet("select")]
        public void SelectCharacter(string packet)
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
                    LastPulse = 0,
                    LastPortal = 0,
                    Invisible = 0,
                    ArenaWinner = 0,
                    Morph = 0,
                    MorphUpgrade = 0,
                    MorphUpgrade2 = 0,
                    Direction = 0,
                    Rested = 0,
                    BackPack = characterDTO.Backpack,
                    Speed = ServersData.SpeedData[characterDTO.Class]
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

            ChatManager.Instance.Broadcast(Session,
                Session.Character.GenerateSay(message, 0),
                ReceiverType.AllOnMapExceptMe);
        }
        [Packet("walk")]
        public void Walk(string packet)
        {

            string[] packetsplit = packet.Split(' ');

            Session.Character.MapX = Convert.ToInt16(packetsplit[2]);
            Session.Character.MapY = Convert.ToInt16(packetsplit[3]);

            ChatManager.Instance.Broadcast(Session,
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
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099),
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
                Session.Client.SendPacket(String.Format("say 1 {0} 1 {1}", Session.Character.CharacterId, Language.Instance.GetMessageFromKey("CANT_MOVE")));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_MOVE"), 2));

            }
        }
        [Packet("rest")]
        public void Rest(string packet)
        {
            Session.Character.Rested = Session.Character.Rested == 1 ? 0 : 1;



            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);

        }
        [Packet("dir")]
        public void Dir(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (Convert.ToInt32(packetsplit[4]) == Session.Character.CharacterId)
            {
                Session.Character.Direction = Convert.ToInt32(packetsplit[2]);
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateDir(), ReceiverType.AllOnMap);

            }
        }
        [Packet("u_s")]
        public void UseSkill(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            ChatManager.Instance.Broadcast(Session, String.Format("cancel 2 {0}", packetsplit[4]), ReceiverType.OnlyMe);

        }
        [Packet("ncif")]
        public void GetNamedCharacterInformation(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (packetsplit[2] == "1")
            {
                ChatManager.Instance.RequiereBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateStatInfo");
            }
            if (packetsplit[2] == "2")
            {
                foreach (Npc npc in ServerManager.GetMap(Session.Character.MapId).Npcs)
                    if (npc.NpcId == Convert.ToInt16(packetsplit[3]))
                        ChatManager.Instance.Broadcast(Session, String.Format("st 2 {0} {1} 100 100 50000 50000", packetsplit[3], npc.Level), ReceiverType.OnlyMe);
            }
        }
        [Packet("game_start")]
        public void StartGame(string packet)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & DAOFactory.GeneralLogDAO.LoadByLogType("Connexion", Session.Character.CharacterId).Count() == 1)
                Session.Client.SendPacket("scene 40");


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
        [Packet("npc_req")]
        public void NpcReq(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            foreach (Npc npc in ServerManager.GetMap(Session.Character.MapId).Npcs)
                if (npc.NpcId == Convert.ToInt16(packetsplit[3]))
                    if (npc.GetNpcDialog() != String.Empty)
                        Session.Client.SendPacket(npc.GetNpcDialog());


        }
        [Packet("b_i")]
        public void askToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            Session.Client.SendPacket(Session.Character.GenerateDialog(String.Format("#b_i^{0}^{1}^1 #b_i^0^0^5 {2}",type, slot,Language.Instance.GetMessageFromKey("ASK_TO_DELETE"))));
           // DeleteItem(type, slot);
        }
        [Packet("#b_i")]
        public void answerToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ','^');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);

            if (Convert.ToInt32(packetsplit[4]) == 1)
            {
                Session.Client.SendPacket(Session.Character.GenerateDialog(String.Format("#b_i^{0}^{1}^2 #b_i^0^0^5 {2}", type, slot,Language.Instance.GetMessageFromKey("SURE_TO_DELETE"))));
            }
            else if (Convert.ToInt32(packetsplit[4]) == 2)
            {
                DeleteItem( type, slot);
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
            InventoryDTO inv = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, slot, type);
            ItemInstanceDTO item = DAOFactory.ItemInstanceDAO.LoadById(inv.ItemInstanceId);
            ItemDTO iteminfo = DAOFactory.ItemDAO.LoadById(item.ItemVNum);
            InventoryDTO invdest = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, destslot, desttype);
            if(invdest == null && ((slot == 6 && iteminfo.ItemType == 4) ||( slot==7 && iteminfo.ItemType == 2) || slot == 0))
            {
                inv.Slot = destslot;
                inv.Type = desttype;
                DAOFactory.InventoryDAO.InsertOrUpdate(ref inv);
            }

        }
        [Packet("get")]
        public void GetItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long DropId; long.TryParse(packetsplit[4], out DropId);
            MapItem mapitem;
            ItemInstanceDTO item;
            if (Session.CurrentMap.DroppedList.TryGetValue(DropId, out mapitem))
            {
                ItemDTO itemInfo = DAOFactory.ItemDAO.LoadById(mapitem.ItemVNum);
                IEnumerable<ItemInstanceDTO> slotfree =  DAOFactory.ItemInstanceDAO.LoadBySlotAllowed(Session.Character.CharacterId,mapitem.ItemVNum, mapitem.Amount);
                List<short> iteminstanceids = new List<short>();
                foreach(ItemInstanceDTO itemfree in slotfree)
                {
                    iteminstanceids.Add(itemfree.ItemInstanceId);
                }
             InventoryDTO invtest=   DAOFactory.InventoryDAO.getFirstSlot(iteminstanceids);
                if (invtest == null || invtest.Type == 0)
                {

                    if (mapitem.PositionX < Session.Character.MapX + 3 && mapitem.PositionX > Session.Character.MapX - 3 && mapitem.PositionY < Session.Character.MapY + 3 && mapitem.PositionY > Session.Character.MapY - 3)
                    {
                        Session.CurrentMap.DroppedList.Remove(DropId);
                        ChatManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                        ItemInstanceDTO newItem = new ItemInstanceDTO()
                        {
                            Amount = mapitem.Amount,
                            ItemVNum = mapitem.ItemVNum,
                            Rare = mapitem.Rare,
                            Upgrade = mapitem.Upgrade,
                            Color = mapitem.Color,
                            Concentrate = mapitem.Concentrate,
                            CriticalLuckRate = mapitem.CriticalLuckRate,
                            CriticalRate = mapitem.CriticalLuckRate,
                            DamageMaximum = mapitem.DamageMaximum,
                            DamageMinimum = mapitem.DamageMinimum,
                            DarkElement = mapitem.DarkElement,
                            DistanceDefence = mapitem.DistanceDefence,
                            Dodge = mapitem.Dodge,
                            ElementRate = mapitem.ElementRate,
                            FireElement = mapitem.FireElement,
                            HitRate = mapitem.HitRate,
                            LightElement = mapitem.LightElement,
                            MagicDefence = mapitem.MagicDefence,
                            RangeDefence = mapitem.RangeDefence,
                            SlDefence = mapitem.SlDefence,
                            SlElement = mapitem.SlElement,
                            SlHit = mapitem.SlHit,
                            SlHP = mapitem.SlHP,
                            WaterElement = mapitem.WaterElement,

                        };
                        SaveResult insertResult = DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref newItem);
                        InventoryDTO newInventory = new InventoryDTO()
                        {
                            CharacterId = Session.Character.CharacterId,
                            ItemInstanceId = newItem.ItemInstanceId,
                            Slot = DAOFactory.InventoryDAO.getFirstPlace(Session.Character.CharacterId, itemInfo.Type, Session.Character.BackPack),
                            Type = itemInfo.Type

                        };
                        insertResult = DAOFactory.InventoryDAO.InsertOrUpdate(ref newInventory);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemVNum, newItem.Amount, newInventory.Type, newInventory.Slot, newItem.Rare, newItem.Color, newItem.Upgrade));
                        Session.Client.SendPacket(Session.Character.GenerateSay(String.Format("{0}: {1} x {2}",Language.Instance.GetMessageFromKey("YOU_GET_OBJECT"),itemInfo.Name,newItem.Amount), 12));
                    }

                }
                else
                {
                    item = DAOFactory.ItemInstanceDAO.LoadById(invtest.ItemInstanceId);
                   InventoryDTO inv= DAOFactory.InventoryDAO.LoadByItemInstance(invtest.ItemInstanceId);
             
                    if (item.ItemVNum == mapitem.ItemVNum)
                    {
                        if (item.Amount + mapitem.Amount < 100)
                        {
                            item.Amount = (short)(item.Amount + mapitem.Amount);
                            DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref item);
                            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format("{0}: {1} x {2}", Language.Instance.GetMessageFromKey("YOU_GET_OBJECT"), itemInfo.Name, mapitem.Amount), 12));

                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, inv.Type, inv.Slot, item.Rare, item.Color, item.Upgrade));
                            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                            Session.CurrentMap.DroppedList.Remove(DropId);
                        }
                    }
                }
            }
        }
        [Packet("put")]
        public void PutItem(string packet)
        {
            Random rnd = new Random();
            int random = 0;
            string[] packetsplit = packet.Split(' ');
            short type; short.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            short amount; short.TryParse(packetsplit[4], out amount);
            InventoryDTO inv = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, slot, type);
            ItemInstanceDTO item = DAOFactory.ItemInstanceDAO.LoadById(inv.ItemInstanceId);
            MapItem DroppedItem;
            if (amount <= item.Amount)
            {
                DroppedItem = new MapItem((short)(rnd.Next(Session.Character.MapX - 2, Session.Character.MapX+3)), (short)(rnd.Next(Session.Character.MapY - 2, Session.Character.MapY + 3)))
                {
                    Amount = amount,
                    Color = item.Color,
                    Concentrate = item.Concentrate,
                    CriticalLuckRate = item.CriticalLuckRate,
                    CriticalRate = item.CriticalRate,
                    DamageMaximum = item.DamageMaximum,
                    DamageMinimum = item.DamageMinimum,
                    DarkElement = item.DarkElement,
                    DistanceDefence = item.DistanceDefence,
                    Dodge = item.Dodge,
                    ElementRate = item.ElementRate,
                    FireElement = item.FireElement,
                    HitRate = item.HitRate,
                    WaterElement = item.WaterElement,
                    SlHit = item.SlHit,
                    ItemVNum = item.ItemVNum,
                    LightElement = item.LightElement,
                    MagicDefence = item.MagicDefence,
                    RangeDefence = item.RangeDefence,
                    Rare = item.Rare,
                    SlDefence = item.SlDefence,
                    SlElement = item.SlElement,
                    SlHP = item.SlHP,
                    Upgrade = item.Upgrade
                };
                while(Session.CurrentMap.DroppedList.ContainsKey(random = rnd.Next(1, 999999)))
                {}
                Session.CurrentMap.DroppedList.Add(random, DroppedItem);
                item.Amount = (short)(item.Amount - amount);
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref item);
                if (item.Amount == 0)
                {
                    DeleteItem(type, inv.Slot);
                }
                else
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, type, inv.Slot, item.Rare, item.Color, item.Upgrade));
                ChatManager.Instance.Broadcast(Session,String.Format("drop {0} {1} {2} {3} {4} {5} {6}", DroppedItem.ItemVNum, random,DroppedItem.PositionX,DroppedItem.PositionY,DroppedItem.Amount,0,-1),ReceiverType.AllOnMap);
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
            InventoryDTO inv = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, slot, type);
            ItemInstanceDTO item = DAOFactory.ItemInstanceDAO.LoadById(inv.ItemInstanceId);
            InventoryDTO invdest = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, destslot, type);
            if (amount <= item.Amount)
            { 
            if (invdest == null)
            {
                if(item.Amount == amount) {
                inv.Slot = destslot;
                DAOFactory.InventoryDAO.InsertOrUpdate(ref inv);
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, type, destslot, item.Rare, item.Color, item.Upgrade));

                }
                else
                {
                        item.Amount = (short)(item.Amount - amount);
                        ItemInstanceDTO itemDest = new ItemInstanceDTO
                        {
                            Amount = amount,
                            Color = item.Color,
                            Concentrate = item.Concentrate,
                            CriticalLuckRate = item.CriticalLuckRate,
                            CriticalRate = item.CriticalRate,
                            DamageMaximum = item.DamageMaximum,
                            DamageMinimum = item.DamageMinimum,
                            DarkElement = item.DarkElement,
                            DistanceDefence = item.DistanceDefence,
                            Dodge = item.Dodge,
                            ElementRate = item.ElementRate,
                            FireElement = item.FireElement,
                            HitRate = item.HitRate,
                            ItemVNum = item.ItemVNum,
                            LightElement = item.LightElement,
                            MagicDefence = item.MagicDefence,
                            RangeDefence = item.RangeDefence,
                            Rare = item.Rare,
                            SlDefence = item.SlDefence
                            ,
                            SlElement = item.SlElement,
                            SlHit = item.SlHit,
                            SlHP = item.SlHP,
                            Upgrade = item.Upgrade,
                            WaterElement = item.WaterElement


                        };
                    
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref item);
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref itemDest);
                        InventoryDTO invDest = new InventoryDTO
                        {
                            CharacterId = Session.Character.CharacterId,
                            ItemInstanceId = itemDest.ItemInstanceId,
                            Slot = destslot,
                            Type = inv.Type
                        };
                        DAOFactory.InventoryDAO.InsertOrUpdate(ref invDest);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, type, inv.Slot, item.Rare, item.Color, item.Upgrade));
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(itemDest.ItemVNum, itemDest.Amount, type, invDest.Slot, itemDest.Rare, itemDest.Color, itemDest.Upgrade));

                    }
                }
                else
            {
                ItemInstanceDTO itemDest = DAOFactory.ItemInstanceDAO.LoadById(invdest.ItemInstanceId);

                if (itemDest.ItemVNum == item.ItemVNum && inv.Type != 0)
                {
                  
                    if (itemDest.Amount + amount > 99)
                    {
                        short saveItemCount = itemDest.Amount;
                        itemDest.Amount = 99;
                        item.Amount = (short)(saveItemCount + item.Amount - 99);
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref item);
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref itemDest);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, type, inv.Slot, item.Rare, item.Color, item.Upgrade));
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(itemDest.ItemVNum, itemDest.Amount, type, invdest.Slot, itemDest.Rare, itemDest.Color, itemDest.Upgrade));

                    }
                    else
                    {
                            short saveItemCount = itemDest.Amount;
                            itemDest.Amount = (short)(saveItemCount+amount);
                            item.Amount = (short)(item.Amount - amount);
                            DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref item);
                            DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref itemDest);
                            if (item.Amount == 0)
                            {
                                DeleteItem(type, slot);

                            }
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, type, inv.Slot, item.Rare, item.Color, item.Upgrade));
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(itemDest.ItemVNum, itemDest.Amount, type, invdest.Slot, itemDest.Rare, itemDest.Color, itemDest.Upgrade));

                        }
                    }
                else
                {
                    invdest.Slot = inv.Slot;
                    inv.Slot = 99;
                    DAOFactory.InventoryDAO.InsertOrUpdate(ref inv);
                    DAOFactory.InventoryDAO.InsertOrUpdate(ref invdest);
                    inv.Slot = destslot;
                    DAOFactory.InventoryDAO.InsertOrUpdate(ref inv);
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, type, inv.Slot, item.Rare, item.Color, item.Upgrade));
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(itemDest.ItemVNum, itemDest.Amount, type, invdest.Slot, itemDest.Rare, itemDest.Color, itemDest.Upgrade));

                }
            }
        }
        }
        [Packet("req_info")]
        public void ReqInfo(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            ChatManager.Instance.RequiereBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateReqInfo");
        }
        [Packet("/")]
        public void Whisper(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";
            message.Trim();

            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlyMe);
            if (!ChatManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlySomeone, packetsplit[1].Substring(1)))
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")), ReceiverType.OnlyMe);

        }
        #endregion

        #region AdminCommand
        [Packet("$Command")]
        public void Command(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$LevelUp LEVEL", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Kick USERNAME", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$MapDance", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic MUSIC", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Invisible", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Position", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 0));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shutdown", 0));
        }
        [Packet("$CreateItem")]
        public void CreateItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short amount = 1;
            short vnum, rare = 0, upgrade = 0, color = 0;
            ItemDTO iteminfo = null;
            if(packetsplit.Length != 3 && packetsplit.Length != 4)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 0));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 0));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 0));
            }
            else if (Int16.TryParse(packetsplit[2], out vnum))
            {
                iteminfo = DAOFactory.ItemDAO.LoadById(vnum);
                if(iteminfo != null)
                {
                if ( iteminfo.Colored)
                {
                    Int16.TryParse(packetsplit[3], out color);
                }
                else if(iteminfo.Type ==0)
                {
                    Int16.TryParse(packetsplit[3], out rare);
                    Int16.TryParse(packetsplit[4], out upgrade);
                }
                else
                {
                    Int16.TryParse(packetsplit[3], out amount);
                }
                ItemInstanceDTO newItem = new ItemInstanceDTO()
                {
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
                    Dodge = 0,
                    ElementRate = 0,
                    FireElement = 0,
                    HitRate = 0,
                    LightElement = 0,
                    MagicDefence = 0,
                    RangeDefence = 0,
                    SlDefence = 0,
                    SlElement = 0,
                    SlHit = 0,
                    SlHP = 0,
                    WaterElement = 0,
                };
                    short Slot = -1;
                    Slot = DAOFactory.InventoryDAO.getFirstPlace(Session.Character.CharacterId, iteminfo.Type, Session.Character.BackPack);
                    if (Slot != -1)
                    {
                        SaveResult insertResult = DAOFactory.ItemInstanceDAO.InsertOrUpdate(ref newItem);
                        InventoryDTO newInventory = new InventoryDTO()
                        {
                            CharacterId = Session.Character.CharacterId,
                            ItemInstanceId = newItem.ItemInstanceId,
                            Slot = Slot,
                            Type = iteminfo.Type

                        };
                        insertResult = DAOFactory.InventoryDAO.InsertOrUpdate(ref newInventory);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(vnum, amount, iteminfo.Type, Slot,rare,color,upgrade));
                    }

                }
                }
        }
        [Packet("$Position")]
        public void Position(string packet)
        {

            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format("Map:{0} - X:{1} - Y:{2}", Session.Character.MapId, Session.Character.MapX, Session.Character.MapY), 0));

        }


        [Packet("$Kick")]
        public void Kick(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            ChatManager.Instance.Kick(packetsplit[2]);

        }
        [Packet("$ChangeClass")]
        public void ChangeClass(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            byte classe;
            if (packetsplit.Length > 3)
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 0));
            if (Byte.TryParse(packetsplit[2], out classe) && classe < 4)
            {
                Session.Client.SendPacket("npinfo 0");
                Session.Client.SendPacket("p_clear");

                Session.Character.Class = classe;
                Session.Character.Speed = ServersData.SpeedData[Session.Character.Class];
                Session.Character.Hp = (int)Session.Character.HPLoad();
                Session.Character.Mp = (int)Session.Character.MPLoad();
                Session.Client.SendPacket(Session.Character.GenerateTit());

                // eq 37 0 1 0 9 3 -1.120.46.86.-1.-1.-1.-1 0 0
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);

                //equip 0 0 0.46.0.0.0 1.120.0.0.0 5.86.0.0.0

                Session.Client.SendPacket(Session.Character.GenerateLev());
                Session.Client.SendPacket(Session.Character.GenerateStat());
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEff(8), ReceiverType.AllOnMap);
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOB_CHANGED"), 0));
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);
                Random rand = new Random();
                int faction = 1 + (int)rand.Next(0, 2);
                Session.Character.Faction = faction;
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(String.Format("GET_PROTECTION_POWER_{0}", faction)), 0));
                Session.Client.SendPacket("scr 0 0 0 0 0 0");

                Session.Client.SendPacket(Session.Character.GenerateFaction());
                // fs 1

                Session.Client.SendPacket(Session.Character.GenerateEff(4799 + faction));
            }
        }
        [Packet("$LevelUp")]
        public void LevelUp(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            byte level;
            if (packetsplit.Length > 3)
                Session.Client.SendPacket(Session.Character.GenerateSay("$LevelUp LEVEL", 0));
            if (Byte.TryParse(packetsplit[2], out level) && level < 100 && level > 0)
            {

                Session.Character.Level = level;
                Session.Character.Hp = (int)Session.Character.HPLoad();
                Session.Character.Mp = (int)Session.Character.MPLoad();
                Session.Client.SendPacket(Session.Character.GenerateStat());
                //sc 0 0 31 39 31 4 70 1 0 33 35 43 2 70 0 17 35 19 35 17 0 0 0 0
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                Session.Client.SendPacket(Session.Character.GenerateLev());
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
            }
        }
        [Packet("$Ban")]
        public void Ban(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            ChatManager.Instance.Kick(packetsplit[2]);
            if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                DAOFactory.AccountDAO.ToggleBan(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId);



        }
        [Packet("$Shutdown")]
        public void Shutdown(string packet)
        {
            if (ChatManager.Instance.shutdownActive == false)
            {
                Thread ThreadShutdown = new Thread(new ThreadStart(ShutdownThread));
                ThreadShutdown.Start();
                ChatManager.Instance.shutdownActive = true;
            }

        }
       
        [Packet("$Shout")]
        public void Shout(string packet)
        {

            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";
            message.Trim();

            ChatManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);

        }

        [Packet("$MapDance")]
        public void MapDance(string packet)
        {
            Session.CurrentMap.IsDancing = Session.CurrentMap.IsDancing == 0 ? 2 : 0;
            if (Session.CurrentMap.IsDancing == 2)
            {
                Session.Character.Dance();
                ChatManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "Dance");
                ChatManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 2");
            }
            else
            {
                Session.Character.Dance();
                ChatManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "Dance");
                ChatManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 0");
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
            if (packetsplit.Length > 1)
            {
                short.TryParse(packetsplit[2], out arg);
                ChatManager.Instance.Broadcast(Session, Session.Character.GenerateEff(arg), ReceiverType.AllOnMap);
            }

        }
        [Packet("$PlayMusic")]
        public void PlayMusic(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = -1;
            if (packetsplit.Length > 1)
            {
                short.TryParse(packetsplit[2], out arg);
                if (arg > -1)
                    ChatManager.Instance.Broadcast(Session, String.Format("bgm {0}", arg), ReceiverType.AllOnMap);
            }

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
                        Session.Character.Morph = arg[0];
                        Session.Character.MorphUpgrade = arg[1];
                        Session.Character.MorphUpgrade2 = arg[2];
                        Session.Character.ArenaWinner = arg[3];
                        ChatManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);

                    }
                    break;
                default:
                    Session.Client.SendPacket(String.Format("say 1 {0} 1 $Morph MORPHID UPGRADE WINGS ARENA", Session.Character.CharacterId));
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
                    Session.Client.SendPacket(String.Format("say 1 {0} 1 $Teleport Map X Y", Session.Character.CharacterId));
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
                    Session.Client.SendPacket(String.Format("say 1 {0} 1  $Speed SPEED", Session.Character.CharacterId));
                    break;
            }

        }
        #endregion
        #region Methods
        public void MapOut()
        {
            Session.Client.SendPacket(Session.Character.GenerateMapOut());
            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
        }
        public void ChangeMap()
        {
            Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);
            Session.Client.SendPacket(Session.Character.GenerateCInfo());
            Session.Client.SendPacket(Session.Character.GenerateFaction());
            Session.Client.SendPacket(Session.Character.GenerateFd());
            Session.Client.SendPacket(Session.Character.GenerateLev());
            Session.Client.SendPacket(Session.Character.GenerateStat());
            //ski
            Session.Client.SendPacket(Session.Character.GenerateAt());
            Session.Client.SendPacket(Session.Character.GenerateCMap());
            foreach (String portalPacket in Session.Character.GenerateGp())
                Session.Client.SendPacket(portalPacket);
            foreach (String npcPacket in Session.Character.Generatein2())
                Session.Client.SendPacket(npcPacket);
            foreach (String droppedPacket in Session.Character.GenerateDroppedItem())
                Session.Client.SendPacket(droppedPacket);

            //sc
            Session.Client.SendPacket(Session.Character.GenerateCond());
            //pairyz
            Session.Client.SendPacket(String.Format("rsfi {0} {1} {2} {3} {4} {5}", 1, 1, 4, 9, 4, 9));//stone act
            ChatManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "GenerateIn");
            ChatManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "GenerateCMode");

            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMap);
            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            if (Session.CurrentMap.IsDancing == 2 && Session.Character.IsDancing == 0)
                ChatManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 2");
            else if (Session.CurrentMap.IsDancing == 0 && Session.Character.IsDancing == 1)
            {
                Session.Character.IsDancing = 0;
                ChatManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 0");

            }

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
                    ChatManager.Instance.Broadcast(Session,
         Session.Character.GenerateStat(),
           ReceiverType.AllOnMap);
                }


            }
        }

        public void ShutdownThread()
        {
            string message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            ChatManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            Thread.Sleep(60000 * 4);
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            ChatManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            Thread.Sleep(30000);
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            ChatManager.Instance.Broadcast(Session, String.Format("say 1 0 10 ({0}){1}", Language.Instance.GetMessageFromKey("ADMINISTRATOR"), message), ReceiverType.All);
            ChatManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            Thread.Sleep(30000);
            //save
            Environment.Exit(0);
        }
        public void DeleteItem(short type, short slot)
        {

            InventoryDTO inv = DAOFactory.InventoryDAO.LoadBySlotAndType(Session.Character.CharacterId, slot, type);
            DAOFactory.InventoryDAO.DeleteFromSlotAndType(Session.Character.CharacterId, slot, type);
            DAOFactory.ItemInstanceDAO.DeleteById(inv.ItemInstanceId);
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));

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
