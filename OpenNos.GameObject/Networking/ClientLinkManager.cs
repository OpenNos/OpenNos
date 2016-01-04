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
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ClientLinkManager
    {
        private static ClientLinkManager _instance;
        public List<ClientSession> sessions { get; set; }
        public bool shutdownActive
        {
            get; set;
        }
        private ClientLinkManager()
        {
            sessions = new List<ClientSession>();
        }
        public static ClientLinkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ClientLinkManager();

                return _instance;
            }
        }
        public bool Broadcast(ClientSession client, String message, ReceiverType receiver, String CharacterName = "", long CharacterId = -1)
        {
            switch (receiver)
            {
                case ReceiverType.All:
                    foreach (ClientSession session in sessions)
                    {
                        session.Client.SendPacket(message);
                    }

                    break;

                case ReceiverType.AllExceptMe:
                    foreach (ClientSession session in sessions)
                    {
                        if (session != client)
                            session.Client.SendPacket(message);
                    }
                    break;

                case ReceiverType.AllOnMap:
                    foreach (ClientSession session in sessions)
                    {
                        if (session.Character != null && client.Character.MapId == session.Character.MapId)
                            session.Client.SendPacket(message);
                    }

                    break;
                case ReceiverType.AllOnMapExceptMe:
                    foreach (ClientSession session in sessions)
                    {
                        if (session.Character != null && session != client && client.Character.MapId == session.Character.MapId)
                            session.Client.SendPacket(message);
                    }
                    break;
                case ReceiverType.OnlyMe:
                    client.Client.SendPacket(message);
                    break;
                case ReceiverType.OnlySomeone:
                    foreach (ClientSession session in sessions)
                    {
                        if (session.Character != null && (session.Character.Name == CharacterName || session.Character.CharacterId == CharacterId))
                        {
                            session.Client.SendPacket(message);
                            return true;
                        }

                    }
                    return false;

            }
            return true;
        }
        public void RequiereBroadcastFromMap(short MapId, string Message)
        {
            foreach (ClientSession session in sessions)
            {
                if (session.Character != null && session.Character.MapId == MapId)
                    Broadcast(session, String.Format(Message, session.Character.CharacterId), ReceiverType.AllOnMap);
            }

        }
        public void RequiereBroadcastFromAllMapUsers(ClientSession client, string methodName)
        {
            foreach (ClientSession session in sessions)
            {

                if (session.Character != null && session.Character.Name != client.Character.Name)
                {
                    Type t = session.Character.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }
            }
        }
        public void RequiereBroadcastFromUser(ClientSession client, long CharacterId, string methodName)
        {
            foreach (ClientSession session in sessions)
            {

                if (session.Character != null && session.Character.CharacterId == CharacterId)
                {
                    Type t = session.Character.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }
            }
        }
        public void RequiereBroadcastFromUser(ClientSession client, string CharacterName, string methodName)
        {
            foreach (ClientSession session in sessions)
            {

                if (session.Character != null && session.Character.Name == CharacterName)
                {
                    Type t = session.Character.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }

            }
        }
        public bool Kick(String CharacterName)
        {
            foreach (ClientSession session in sessions)
            {
                if (session.Character != null && session.Character.Name == CharacterName)
                {
                    session.Client.Disconnect();
                    return true;
                }
            }
            return false;
        }
        public object RequiereProperties(long charId, string properties)
        {
            foreach (ClientSession session in sessions)
            {

                if (session.Character != null && session.Character.CharacterId == charId)
                {
                    return session.Character.GetType().GetProperties().Single(pi => pi.Name == properties).GetValue(session.Character, null);
                }
            }
            return "";
        }
        public void BuyValidate(ClientSession Session, KeyValuePair<long, MapShop> shop,short slot, short amount)
        {
            ShopItem itemshop =  Session.CurrentMap.ShopUserList[shop.Key].Items.FirstOrDefault(i => i.Slot.Equals(slot));
            Session.CurrentMap.ShopUserList[shop.Key].Items.FirstOrDefault(i => i.Slot.Equals(slot)).Amount -= amount;
            ShopItem itemDelete = Session.CurrentMap.ShopUserList[shop.Key].Items.FirstOrDefault(i => i.Slot.Equals(slot));
            if (itemDelete.Amount <= 0)
                Session.CurrentMap.ShopUserList[shop.Key].Items.Remove(itemDelete);

            foreach (ClientSession session in sessions)
            {

                if (session.Character != null && session.Character.CharacterId == shop.Value.OwnerId)
                {
                    session.Character.Gold += itemshop.Price* amount;
                    session.Client.SendPacket(session.Character.GenerateGold());
                    session.Client.SendPacket(session.Character.GenerateShopMemo(1, String.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), session.Character.Name, ServerManager.GetItem(itemshop.ItemVNum).Name, amount)));
                    Session.CurrentMap.ShopUserList[shop.Key].Sell += itemshop.Price * amount;
                    session.Client.SendPacket(String.Format("sell_list {0} {1}.{2}.{3}", shop.Value.Sell, slot, amount, itemshop.Amount));
                  Inventory inv = session.Character.InventoryList.AmountMinusFromSlotAndType(amount, itemshop.InvSlot, itemshop.InvType);
                    if(inv != null)
                    {
                        session.Client.SendPacket(session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Color, inv.InventoryItem.Upgrade));
                    }
                    else
                        session.Client.SendPacket(session.Character.GenerateInventoryAdd(-1, 0, itemshop.InvType, itemshop.InvSlot, 0, 0, 0));

                }
            }
        }
        public void ExchangeValidate(ClientSession Session, long charId)
        {
            foreach (ClientSession session in sessions)
            {
               
                if (session.Character != null && session.Character.CharacterId == charId)
                {
                    foreach (InventoryItem item in session.Character.ExchangeInfo.ExchangeList)
                    {
                        Inventory inv = session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                        session.Character.InventoryList.DeleteByInventoryItemId(item.InventoryItemId);
                        session.Client.SendPacket(session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));

                    }
                    foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                    {
                        Inventory inv = session.Character.InventoryList.CreateItem(item, Session.Character);
                        if (inv != null)
                        {
                            short Slot = inv.Slot;
                            if (Slot != -1)
                                session.Client.SendPacket(session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, inv.Type, Slot, inv.InventoryItem.Rare, inv.InventoryItem.Color, inv.InventoryItem.Upgrade));
                        }
                    }
                    session.Character.Gold = session.Character.Gold - session.Character.ExchangeInfo.Gold + Session.Character.ExchangeInfo.Gold;
                    session.Client.SendPacket(session.Character.GenerateGold());
                }
            }
        }
    }
}
