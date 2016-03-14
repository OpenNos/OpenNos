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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System;
using OpenNos.Data;
using OpenNos.DAL;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ClientLinkManager
    {
        #region Members
        private static ClientLinkManager _instance;
        private readonly Task _autoSave; // if this thread is never aborted by code, it can be declared only in constructor!
        Task TaskController;
        public Thread threadShutdown
        {
            get; set;
        }

        #endregion

        #region Instantiation
        public async void TaskControl()
        {
            Task TaskMap;
            while (true)
            {
                foreach (var GroupedSession in Sessions.Where(s => s.Character != null).GroupBy(s => s.Character.MapId))
                {
                    foreach (ClientSession Session in GroupedSession)
                    {
                        TaskMap = new Task(() => ServerManager.GetMap(Session.Character.MapId).MapTaskManager());
                        TaskMap.Start();
                    }
                }
                await Task.Delay(1000);
            }
        }
        private ClientLinkManager()
        {
            Sessions = new List<ClientSession>();
            _autoSave = new Task(SaveAllProcess);
            _autoSave.Start();
            TaskController = new Task(() => TaskControl());
            TaskController.Start();
        }

        #endregion

        #region Properties

        public static ClientLinkManager Instance => _instance ?? (_instance = new ClientLinkManager());

        public List<ClientSession> Sessions { get; set; }

        public bool ShutdownActive { get; set; }

        #endregion

        #region Methods

        public void MapOut(long id)
        {
            foreach (ClientSession Session in Sessions.Where(s => s.Character != null && s.Character.CharacterId == id))
            {
                Session.Client.SendPacket(Session.Character.GenerateMapOut());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllOnMapExceptMe);
            }

        }

        public void ClassChange(long id, byte Class)
        {
            foreach (ClientSession Session in Sessions.Where(s => s.Character != null && s.Character.CharacterId == id))
            {
                Session.Character.JobLevel = 1;
                Session.Client.SendPacket("npinfo 0");
                Session.Client.SendPacket("p_clear");

                Session.Character.Class = Class;
                Session.Character.Speed = ServersData.SpeedData[Session.Character.Class];
                Session.Client.SendPacket(Session.Character.GenerateCond());
                Session.Character.Hp = (int)Session.Character.HPLoad();
                Session.Character.Mp = (int)Session.Character.MPLoad();
                Session.Client.SendPacket(Session.Character.GenerateTit());

                //eq 37 0 1 0 9 3 -1.120.46.86.-1.-1.-1.-1 0 0
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
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
                Session.Client.SendPacket("scr 0 0 0 0 0 0");

                Session.Client.SendPacket(Session.Character.GenerateFaction());
                Session.Client.SendPacket(Session.Character.GenerateStatChar());

                Session.Client.SendPacket(Session.Character.GenerateEff(4799 + faction));
                Session.Client.SendPacket(Session.Character.GenerateLev());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
            }
        }
        public void ChangeMap(long id)
        {
            foreach (ClientSession Session in Sessions.Where(s => s.Character != null && s.Character.CharacterId == id))
            {
                Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);
                Session.Client.SendPacket(Session.Character.GenerateCInfo());
                Session.Client.SendPacket(Session.Character.GenerateCMode());
                Session.Client.SendPacket(Session.Character.GenerateFaction());
                Session.Client.SendPacket(Session.Character.GenerateFd());
                Session.Client.SendPacket(Session.Character.GenerateLev());
                Session.Client.SendPacket(Session.Character.GenerateStat());
                // ski
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

                Session.Client.SendPacket(Session.Character.GenerateStatChar());
                Session.Client.SendPacket(Session.Character.GenerateCond());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                Session.Client.SendPacket($"rsfi 1 1 0 9 0 9"); // Act completion
                ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsersNotInvisible(Session, "GenerateIn");
                if (Session.Character.InvisibleGm == false)
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
                string clinit = "clinit";
                string flinit = "flinit";
                string kdlinit = "kdlinit";

                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopComplimented())
                {
                    clinit += $" {character.CharacterId}|{character.Level}|{character.Compliment}|{character.Name}";
                }
                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopReputation())
                {
                    flinit += $" {character.CharacterId}|{character.Level}|{character.Reput}|{character.Name}";
                }
                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopPoints())
                {
                    kdlinit += $" {character.CharacterId}|{character.Level}|{character.Act4Points}|{character.Name}";
                }

                Session.Client.SendPacket(clinit);
                Session.Client.SendPacket(flinit);
                Session.Client.SendPacket(kdlinit);
            }

        }

        public bool Broadcast(ClientSession client, string message, ReceiverType receiver, string characterName = "", long characterId = -1)
        {
            switch (receiver)
            {
                case ReceiverType.All:
                    foreach (ClientSession session in Sessions)
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.AllExceptMe:
                    foreach (ClientSession session in Sessions.Where(c => c != client))
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.AllOnMap:
                    foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(client.Character.MapId)))
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.AllOnMapExceptMe:
                    foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(client.Character.MapId) && s.Character.CharacterId != client.Character.CharacterId))
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.OnlyMe:
                    client.Client.SendPacket(message);
                    break;

                case ReceiverType.OnlySomeone:
                    ClientSession targetSession = Sessions.FirstOrDefault(s => s.Character != null && (s.Character.Name.Equals(characterName) || s.Character.CharacterId.Equals(characterId)));

                    if (targetSession == null) return false;

                    targetSession.Client.SendPacket(message);
                    return true;

                case ReceiverType.AllOnMapNoEmoBlocked:
                    foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(client.Character.MapId) && !s.Character.EmoticonsBlocked))
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.AllNoHeroBlocked:
                    foreach (ClientSession session in Sessions.Where(s => s.Character != null && !s.Character.HeroChatBlocked))
                        session.Client.SendPacket(message);
                    break;
            }
            return true;
        }

        public void BuyValidate(ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            PersonalShopItem itemshop = clientSession.CurrentMap.ShopUserList[shop.Key].Items.FirstOrDefault(i => i.Slot.Equals(slot));
            if (itemshop == null)
                return;

            itemshop.Amount -= amount;
            if (itemshop.Amount <= 0)
                clientSession.CurrentMap.ShopUserList[shop.Key].Items.Remove(itemshop);

            ClientSession shopOwnerSession = Sessions.FirstOrDefault(s => s.Character.CharacterId.Equals(shop.Value.OwnerId));
            if (shopOwnerSession == null) return;

            shopOwnerSession.Character.Gold += itemshop.Price * amount;
            shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateShopMemo(1,
                string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), shopOwnerSession.Character.Name, ServerManager.GetItem(itemshop.ItemVNum).Name, amount)));
            clientSession.CurrentMap.ShopUserList[shop.Key].Sell += itemshop.Price * amount;
            shopOwnerSession.Client.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{itemshop.Amount}");

            Inventory inv = shopOwnerSession.Character.InventoryList.AmountMinusFromSlotAndType(amount, itemshop.InvSlot, itemshop.InvType);

            if (inv != null)
            {
                // Send reduced-amount to owners inventory
                shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, inv.Type,
                    inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Design, inv.InventoryItem.Upgrade));
            }
            else
                // Send empty slot to owners inventory
                shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(-1, 0, itemshop.InvType, itemshop.InvSlot, 0, 0, 0));
        }

        public void ExchangeValidate(ClientSession c1Session, long charId)
        {
            ClientSession c2Session = Sessions.FirstOrDefault(s => s.Character.CharacterId.Equals(charId));
            {
                if (c2Session == null) return;

                foreach (InventoryItem item in c2Session.Character.ExchangeInfo.ExchangeList)
                {
                    Inventory inv = c2Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                    c2Session.Character.InventoryList.DeleteByInventoryItemId(item.InventoryItemId);
                    c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));
                }

                foreach (InventoryItem item in c1Session.Character.ExchangeInfo.ExchangeList)
                {
                    Inventory inv = c2Session.Character.InventoryList.CreateItem(item, c1Session.Character);
                    if (inv == null) continue;
                    if (inv.Slot == -1) continue;

                    c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Design, inv.InventoryItem.Upgrade));
                }

                c2Session.Character.Gold = c2Session.Character.Gold - c2Session.Character.ExchangeInfo.Gold + c1Session.Character.ExchangeInfo.Gold;
                c2Session.Client.SendPacket(c2Session.Character.GenerateGold());
            }
        }

        public bool Kick(string characterName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(characterName));
            if (session == null) return false;

            session.Client.Disconnect();
            return true;
        }

        public void RequiereBroadcastFromAllMapUsers(ClientSession client, string methodName)
        {
            foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(client.Character.MapId) && s.Character.Name != client.Character.Name))
            {
                MethodInfo method = session.Character.GetType().GetMethod(methodName);
                string result = (string)method.Invoke(session.Character, null);
                client.Client.SendPacket(result);
            }
        }

        public void RequiereBroadcastFromAllMapUsersNotInvisible(ClientSession client, string methodName)
        {
            foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(client.Character.MapId) && s.Character.Name != client.Character.Name))
            {
                if (session.Character.InvisibleGm == false)
                {
                    MethodInfo method = session.Character.GetType().GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }
            }
        }

        public void RequiereBroadcastFromMap(short mapId, string message)
        {
            foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(mapId)))
                Broadcast(session, string.Format(message, session.Character.CharacterId), ReceiverType.AllOnMap);
        }

        public void RequiereBroadcastFromUser(ClientSession client, long characterId, string methodName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(characterId));
            if (session == null) return;

            MethodInfo method = session.Character.GetType().GetMethod(methodName);
            string result = (string)method.Invoke(session.Character, null);
            client.Client.SendPacket(result);
        }

        public void RequiereBroadcastFromUser(ClientSession client, string characterName, string methodName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(characterName));
            if (session == null) return;

            MethodInfo method = session.Character.GetType().GetMethod(methodName);
            string result = (string)method.Invoke(session.Character, null);
            client.Client.SendPacket(result);
        }

        public T GetProperty<T>(string charName, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetProperty<T>(long charId, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(charId));
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public void SetProperty(long charId, string property, object value)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(charId));
            if (session == null) return;

            PropertyInfo propertyinfo = session.Character.GetType().GetProperties().Single(pi => pi.Name == property);
            propertyinfo.SetValue(session.Character, value, null);

        }
        public void SetProperty(string charName, string property, object value)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            if (session == null) return;

            PropertyInfo propertyinfo = session.Character.GetType().GetProperties().Single(pi => pi.Name == property);
            propertyinfo.SetValue(session.Character, value, null);

        }

        public int GetNumberOfAllASession()
        {
            return Sessions.Count();
        }

        public void SaveAll()
        {
            foreach (ClientSession session in Sessions)
                session.Character?.Save();
        }

        private async void SaveAllProcess()
        {
            while (true)
            {
                SaveAll();
                await Task.Delay(60000 * 4);
            }
        }

        #endregion
    }
}