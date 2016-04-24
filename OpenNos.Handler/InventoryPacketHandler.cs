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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class InventoryPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public InventoryPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session { get { return _session; } }

        #endregion

        #region Methods
        [Packet("eqinfo")]
        public void EqInfo(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length <= 3) return;

            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            Inventory inventory = null;
            switch (type)
            {
                case 0:
                    inventory = Session.Character.EquipmentList.LoadBySlotAndType(slot, (byte)InventoryType.Equipment);
                    break;

                case 5:
                    if (Session.Character.ExchangeInfo != null)
                    {
                        byte inven; ;

                        if (byte.TryParse(packetsplit[3], out inven) && short.TryParse(packetsplit[4], out slot))
                        {
                            InventoryList inv = ClientLinkManager.Instance.GetProperty<InventoryList>(Session.Character.ExchangeInfo.CharId, "InventoryList");
                            inventory = inv.LoadBySlotAndType(slot, inven);
                        }
                    }
                    break;

                case 1:
                    inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, (byte)InventoryType.Wear);
                    break;

                case 2:
                    Item item = ServerManager.GetItem(slot);
                    inventory = new Inventory()
                    {
                        InventoryItem = new InventoryItem()
                        {
                            InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                            Amount = 1,
                            ItemVNum = item.VNum,
                            Rare = 0,
                            Upgrade = 0,
                            Design = 0,
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
                            IsFixed = false,
                            Ammo = 0,
                            MagicDefence = 0,
                            CloseDefence = 0,
                            SpXp = 0,
                            SpLevel = 0,
                            SlDefence = 0,
                            SlElement = 0,
                            SlDamage = 0,
                            SlHP = 0,
                            WaterElement = 0
                        }
                    };
                    break;

                case 10:
                    inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, (byte)InventoryType.Sp);
                    break;

                case 11:
                    inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, (byte)InventoryType.Costume);
                    break;
            }

            if (inventory != null)
            {
                Session.Client.SendPacket(
                    ServerManager.GetItem(inventory.InventoryItem.ItemVNum).EquipmentSlot != (byte)EquipmentType.Sp
                        ? Session.Character.GenerateEInfo(new InventoryItem(inventory.InventoryItem))
                        : Session.Character.GenerateSlInfo(new InventoryItem(inventory.InventoryItem), 0));
            }
        }

        [Packet("mve")]
        public void MoveInventory(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            byte desttype; byte.TryParse(packetsplit[4], out desttype);
            short destslot; short.TryParse(packetsplit[5], out destslot);
            if (destslot > 48 + (Session.Character.BackPack * 12))
                return;
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            Inventory inv = Session.Character.InventoryList.moveInventory(type, slot, desttype, destslot);
            if (inv != null)
            {
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, desttype, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Design, inv.InventoryItem.Upgrade));
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
            }
        }

        [Packet("mvi")]
        public void MoveItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            byte amount; byte.TryParse(packetsplit[4], out amount);
            short destslot; short.TryParse(packetsplit[5], out destslot);
            Inventory LastInventory;
            Inventory NewInventory;
            if (destslot > 48 + (Session.Character.BackPack * 12))
                return;
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            Session.Character.InventoryList.MoveItem(Session.Character, type, slot, amount, destslot, out LastInventory, out NewInventory);
            if (NewInventory == null) return;
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(NewInventory.InventoryItem.ItemVNum, NewInventory.InventoryItem.Amount, type, NewInventory.Slot, NewInventory.InventoryItem.Rare, NewInventory.InventoryItem.Design, NewInventory.InventoryItem.Upgrade));
            if (LastInventory != null)
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(LastInventory.InventoryItem.ItemVNum, LastInventory.InventoryItem.Amount, type, LastInventory.Slot, LastInventory.InventoryItem.Rare, LastInventory.InventoryItem.Design, LastInventory.InventoryItem.Upgrade));
            else
            {
                Session.Character.DeleteItem(Session, type, slot);
            }
        }

        [Packet("put")]
        public void PutItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            byte amount; byte.TryParse(packetsplit[4], out amount);
            Inventory inv;
            Inventory invitem = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
            if (invitem != null && ServerManager.GetItem(invitem.InventoryItem.ItemVNum).IsDroppable == true && ServerManager.GetItem(invitem.InventoryItem.ItemVNum).IsTradable == true && (Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null && (Session.Character.ExchangeInfo == null || Session.Character.ExchangeInfo?.ExchangeList.Count() == 0)))
            {
                if (amount > 0 && amount < 100)
                {
                    MapItem DroppedItem = Session.Character.InventoryList.PutItem(Session, type, slot, amount, out inv);
                    if (DroppedItem == null)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE_HERE"), 0)); ;
                        return;
                    }
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, type, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Design, inv.InventoryItem.Upgrade));

                    if (inv.InventoryItem.Amount == 0)
                        Session.Character.DeleteItem(Session, inv.Type, inv.Slot);
                    if (DroppedItem != null)
                        ClientLinkManager.Instance.Broadcast(Session, $"drop {DroppedItem.ItemVNum} {DroppedItem.InventoryItemId} {DroppedItem.PositionX} {DroppedItem.PositionY} {DroppedItem.Amount} 0 -1", ReceiverType.AllOnMap);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DROP_AMOUNT"), 0));
                }
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
            }
        }

        [Packet("remove")]
        public void Remove(string packet)
        {
            // Undress Equipment
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3 && Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null && (Session.Character.ExchangeInfo == null || Session.Character.ExchangeInfo?.ExchangeList.Count() == 0))
            {
                short slot;
                if (!short.TryParse(packetsplit[2], out slot)) return; // Invalid Number

                Inventory inventory = Session.Character.EquipmentList.LoadBySlotAndType(slot, (byte)InventoryType.Equipment);
                if (inventory == null) return; // This eqslot is not equipped

                if (slot == (byte)EquipmentType.Sp && Session.Character.UseSp)
                {
                    Session.Character.LastSp = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    new Task(() => RemoveSP(inventory.InventoryItem.ItemVNum)).Start();
                }

                // Put item back to inventory
                Inventory inv = Session.Character.InventoryList.CreateItem(new InventoryItem(inventory.InventoryItem), Session.Character);
                if (inv == null) return;

                if (inv.Slot != -1)
                    Session.Client.SendPacket(
                        Session.Character.GenerateInventoryAdd(inventory.InventoryItem.ItemVNum,
                            inv.InventoryItem.Amount, inv.Type, inv.Slot, inventory.InventoryItem.Rare,
                            inventory.InventoryItem.Design, inventory.InventoryItem.Upgrade));

                Session.Character.EquipmentList.DeleteFromSlotAndType(slot, (byte)InventoryType.Equipment);

                Session.Client.SendPacket(Session.Character.GenerateStatChar());
                Thread.Sleep(100);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                Session.Client.SendPacket(Session.Character.GenerateEquipment());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
            }
        }

        [Packet("sortopen")]
        public void SortOpen(string packet)
        {
            Boolean gravity = true;
            byte type;
            while (gravity)
            {
                gravity = false;
                for (short x = 0; x < 44; x++)
                {
                    for (short i = 0; i < 2; i++)
                    {
                        type = (i == 0) ? (byte)InventoryType.Sp : (byte)InventoryType.Costume;
                        if (Session.Character.InventoryList.LoadBySlotAndType(x, type) == null)
                        {
                            if (Session.Character.InventoryList.LoadBySlotAndType((short)(x + 1), type) != null)
                            {
                                Inventory invdest = new Inventory();
                                Inventory inv = new Inventory();
                                Session.Character.InventoryList.MoveItem(Session.Character, type, (short)(x + 1), 1, x, out inv, out invdest);
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(invdest.InventoryItem.ItemVNum, invdest.InventoryItem.Amount, type, invdest.Slot, invdest.InventoryItem.Rare, invdest.InventoryItem.Design, invdest.InventoryItem.Upgrade));
                                Session.Character.DeleteItem(Session, type, (short)(x + 1));
                                gravity = true;
                            }
                        }
                    }
                }
            }
        }
        [Packet("game_start")]
        public void StartGame(string packet)
        {
            if (Session.CurrentMap != null)
                return;
            Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);

            if (System.Configuration.ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & DAOFactory.GeneralLogDAO.LoadByLogType("Connection", Session.Character.CharacterId).Count() == 1) Session.Client.SendPacket("scene 40");
            if (System.Configuration.ConfigurationManager.AppSettings["WorldInformation"].ToLower() == "true")
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("---------- World Information ----------", 10));
                Assembly assembly = Assembly.GetEntryAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Session.Client.SendPacket(Session.Character.GenerateSay($"OpenNos by OpenNos Team\nVersion: v{fileVersionInfo.ProductVersion}", 6));
                Session.Client.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
            }
            Session.Character.LoadSkills();
            Session.Client.SendPacket(Session.Character.GenerateTit());
            if (Session.Character.Hp <= 0)
                ClientLinkManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
            else
                ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);

            Session.Client.SendPacket("rage 0 250000");
            Session.Client.SendPacket("rank_cool 0 0 18000");

            Session.Client.SendPacket("scr 0 0 0 0 0 0");

            Session.Client.SendPacket($"bn 0 {Language.Instance.GetMessageFromKey("BN0")}");
            Session.Client.SendPacket($"bn 1 {Language.Instance.GetMessageFromKey("BN1")}");
            Session.Client.SendPacket($"bn 2 {Language.Instance.GetMessageFromKey("BN2")}");
            Session.Client.SendPacket($"bn 3 {Language.Instance.GetMessageFromKey("BN3")}");
            Session.Client.SendPacket($"bn 4 {Language.Instance.GetMessageFromKey("BN4")}");
            Session.Client.SendPacket($"bn 5 {Language.Instance.GetMessageFromKey("BN5")}");
            Session.Client.SendPacket($"bn 6 {Language.Instance.GetMessageFromKey("BN6")}");

            Session.Client.SendPacket(Session.Character.GenerateExts());
            Session.Client.SendPacket(Session.Character.GenerateGold());

            Session.Client.SendPacket(Session.Character.GenerateSki());

            string[] quicklistpackets = Session.Character.GenerateQuicklist();
            foreach (string quicklist in quicklistpackets)
                Session.Client.SendPacket(quicklist);

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpPoint(), ReceiverType.AllOnMap);
            Session.Character.GetStartupInventory(Session);
            // gidx
            Session.Client.SendPacket($"mlinfo 3800 2000 100 0 0 10 0 {Language.Instance.GetMessageFromKey("WELCOME_MUSIC_INFO")}");
            // cond
            Session.Client.SendPacket("p_clear");
            // sc_p pet
            Session.Client.SendPacket("pinit 0");
            Session.Client.SendPacket("zzim");
            Session.Client.SendPacket($"twk 1 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr");

            Session.Character.DeleteTimeout(Session);
        }


        [Packet("up_gr")]
        public void UpgradeCommand(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            if (packetsplit.Count() > 4)
            {
                byte uptype, type, slot, type2 = 0, slot2 = 0;
                byte.TryParse(packetsplit[2], out uptype);
                byte.TryParse(packetsplit[3], out type);
                byte.TryParse(packetsplit[4], out slot);

                if (packetsplit.Count() > 6)
                {
                    byte.TryParse(packetsplit[5], out type2);
                    byte.TryParse(packetsplit[6], out slot2);
                }
                Inventory inventory;
                switch (uptype)
                {
                    case 1:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Armor || iteminfo.EquipmentSlot == (byte)EquipmentType.MainWeapon || iteminfo.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)
                                inventory.UpgradeItem(Session, UpgradeMode.Normal, UpgradeProtection.None);
                        }
                        break;

                    case 7:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Armor || iteminfo.EquipmentSlot == (byte)EquipmentType.MainWeapon || iteminfo.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)

                                inventory.RarifyItem(Session, RarifyMode.Normal, RarifyProtection.None);
                        }
                        break;

                    case 8:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        Inventory inventory2 = Session.Character.InventoryList.LoadBySlotAndType(slot2, type2);

                        if (inventory != null && inventory2 != null && inventory != inventory2)
                        {
                            inventory.SumItem(Session, inventory2);
                        }
                        break;

                    case 9:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp)

                                inventory.UpgradeSp(Session, UpgradeProtection.None);
                        }
                        break;

                    case 41:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp)

                                inventory.PerfectSP(Session, UpgradeProtection.None);
                        }
                        break;
                }
            }
        }

        [Packet("u_i")]
        public void UseItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[4], out type);
            short slot; short.TryParse(packetsplit[5], out slot);
            Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
            if (inv != null)
            {
                Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                iteminfo.Use(Session, ref inv);
            }
        }
        [Packet("wear")]
        public void Wear(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            if (packetsplit.Length > 3 && Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null)
            {
                byte type;
                short slot;

                if (!byte.TryParse(packetsplit[3], out type) || !short.TryParse(packetsplit[2], out slot)) return;
                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                if (inv != null)
                {
                    Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                    iteminfo.Use(Session, ref inv);
                }
            }
        }

        [Packet("req_exc")]
        public void Exchange(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short mode;
            if (!short.TryParse(packetsplit[2], out mode)) return;

            long charId = -1;
            string charName;
            bool Blocked;

            if (mode == 1)
            {
                if (!long.TryParse(packetsplit[3], out charId)) return;
                Blocked = ClientLinkManager.Instance.GetProperty<bool>(charId, "ExchangeBlocked");

                if (Session.Character.Speed == 0 || ClientLinkManager.Instance.GetProperty<byte>(charId, "Speed") == 0)
                    Blocked = true;
                if (Blocked)
                {
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRADE_BLOCKED"), 11), ReceiverType.OnlyMe);
                }
                else
                {
                    Session.Character.ExchangeInfo = new ExchangeInfo { CharId = charId, Confirm = false };

                    charName = (string)ClientLinkManager.Instance.GetProperty<string>(charId, "Name");
                    Session.Client.SendPacket(Session.Character.GenerateModal($"{Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE")} {charName}"));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateDialog($"#req_exc^2^{Session.Character.CharacterId} #req_exc^5^{Session.Character.CharacterId} {String.Format(Language.Instance.GetMessageFromKey("INCOMING_EXCHANGE"), Session.Character.Name)}"), ReceiverType.OnlySomeone, charName);
                }
            }
            else if (mode == 3)
            {
                if (Session.Character.ExchangeInfo.CharId == Session.Character.CharacterId) return;
                ExchangeInfo exchange = ClientLinkManager.Instance.GetProperty<ExchangeInfo>(Session.Character.ExchangeInfo.CharId, "ExchangeInfo");
                long gold = ClientLinkManager.Instance.GetProperty<long>(Session.Character.ExchangeInfo.CharId, "Gold");
                int backpack = ClientLinkManager.Instance.GetProperty<int>(Session.Character.ExchangeInfo.CharId, "BackPack");
                InventoryList inventory = ClientLinkManager.Instance.GetProperty<InventoryList>(Session.Character.ExchangeInfo.CharId, "InventoryList");
                if (Session.Character.ExchangeInfo.Validate && exchange.Validate)
                {
                    Session.Character.ExchangeInfo.Confirm = true;
                    if (exchange.Confirm)
                    {
                        Session.Client.SendPacket("exc_close 1");
                        ClientLinkManager.Instance.Broadcast(Session, "exc_close 1", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                        bool continu = true;
                        bool goldmax = false;
                        bool notsold = false;
                        if (!Session.Character.InventoryList.getFreePlaceAmount(Session.Character.ExchangeInfo.ExchangeList, Session.Character.BackPack))
                            continu = false;

                        if (!inventory.getFreePlaceAmount(exchange.ExchangeList, backpack))
                            continu = false;

                        if (Session.Character.ExchangeInfo.Gold + gold > 1000000000)
                            goldmax = true;
                        if (Session.Character.ExchangeInfo.Gold > Session.Character.Gold)
                            return;
                        if (exchange.Gold + Session.Character.Gold > 1000000000)
                            goldmax = true;

                        if (continu == false)
                        {
                            string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0);
                            Session.Client.SendPacket(message);
                            ClientLinkManager.Instance.Broadcast(Session, message, ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            Session.Client.SendPacket("exc_close 0");
                            ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                            Session.Character.ExchangeInfo = null;
                        }
                        else if (goldmax == true)
                        {
                            string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                            Session.Client.SendPacket(message);
                            ClientLinkManager.Instance.Broadcast(Session, message, ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            Session.Client.SendPacket("exc_close 0");
                            ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                            Session.Character.ExchangeInfo = null;
                        }
                        else
                        {
                            foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                            {
                                Inventory inv = Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                                if (inv != null && ServerManager.GetItem(inv.InventoryItem.ItemVNum).IsTradable != true)
                                {
                                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_TRADABLE"), 0));
                                    Session.Client.SendPacket("exc_close 0");
                                    ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                                    ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                                    Session.Character.ExchangeInfo = null;
                                    notsold = true;
                                    break;
                                }
                            }
                            if (!notsold)
                            {
                                foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                                {
                                    // Delete items from their owners
                                    Inventory inv = Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                                    Session.Character.InventoryList.DeleteByInventoryItemId(item.InventoryItemId);
                                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));
                                }

                                foreach (InventoryItem item in exchange.ExchangeList)
                                {
                                    // Add items to their new owners
                                    Inventory inv = Session.Character.InventoryList.CreateItem(item, Session.Character);
                                    if (inv != null && inv.Slot != -1)
                                        Session.Client.SendPacket(
                                            Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum,
                                                inv.InventoryItem.Amount, inv.Type, inv.Slot, inv.InventoryItem.Rare,
                                                inv.InventoryItem.Design, inv.InventoryItem.Upgrade));
                                }

                                Session.Character.Gold = Session.Character.Gold - Session.Character.ExchangeInfo.Gold + exchange.Gold;
                                Session.Client.SendPacket(Session.Character.GenerateGold());
                                ClientLinkManager.Instance.ExchangeValidate(Session, Session.Character.ExchangeInfo.CharId);

                                // TODO: Maybe log exchanges to a (new) table, so that the server admins could trace cheaters
                            }
                        }
                    }
                    else
                    {
                        charName = ClientLinkManager.Instance.GetProperty<string>(charId, "Name");

                        Session.Client.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("IN_WAITING_FOR"), charName)));
                    }
                }
            }
            else if (mode == 4)
            {
                Session.Client.SendPacket("exc_close 0");
                ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                Session.Character.ExchangeInfo = null;
            }
        }

        [Packet("exc_list")]
        public void ExchangeList(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long Gold = 0; long.TryParse(packetsplit[2], out Gold);
            byte[] type = new byte[10];
            short[] slot = new short[10];
            byte[] qty = new byte[10];
            string packetList = "";
            for (int j = 6, i = 0; j <= packetsplit.Length; j += 3, i++)
            {
                byte.TryParse(packetsplit[j - 3], out type[i]);
                short.TryParse(packetsplit[j - 2], out slot[i]);
                byte.TryParse(packetsplit[j - 1], out qty[i]);
                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot[i], type[i]);
                InventoryItem item = new InventoryItem(inv.InventoryItem);
                Session.Character.ExchangeInfo.ExchangeList.Add(item);
                item.Amount = qty[i];
                if (type[i] != 0)
                    packetList += $"{i}.{type[i]}.{item.ItemVNum}.{qty[i]} ";
                else
                    packetList += $"{i}.{type[i]}.{item.ItemVNum}.{inv.InventoryItem.Rare}.{inv.InventoryItem.Upgrade} ";
            }
            Session.Character.ExchangeInfo.Gold = Gold;
            ClientLinkManager.Instance.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} {Gold} {packetList}", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);
            Session.Character.ExchangeInfo.Validate = true;
        }

        [Packet("get")]
        public void GetItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long DropId; long.TryParse(packetsplit[4], out DropId);
            MapItem mapitem;
            if (Session.CurrentMap.DroppedList.TryGetValue(DropId, out mapitem))
            {
                int Amount = mapitem.Amount;
                if (mapitem.PositionX < Session.Character.MapX + 3 && mapitem.PositionX > Session.Character.MapX - 3 && mapitem.PositionY < Session.Character.MapY + 3 && mapitem.PositionY > Session.Character.MapY - 3)
                {
                    if (mapitem.ItemVNum != 1046)
                    {
                        Inventory newInv = Session.Character.InventoryList.CreateItem((InventoryItem)mapitem, Session.Character);
                        if (newInv != null)
                        {
                            Session.CurrentMap.DroppedList.Remove(DropId);
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                            Item iteminfo = ServerManager.GetItem(newInv.InventoryItem.ItemVNum);
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newInv.InventoryItem.ItemVNum, newInv.InventoryItem.Amount, newInv.Type, newInv.Slot, newInv.InventoryItem.Rare, newInv.InventoryItem.Design, newInv.InventoryItem.Upgrade));
                            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {Amount}", 12));
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        }
                    }
                    else
                    {
                        if (Session.Character.Gold + mapitem.Amount <= 1000000000)
                        {
                            Item iteminfo = ServerManager.GetItem(mapitem.ItemVNum);
                            Session.Character.Gold += mapitem.Amount;
                            Session.CurrentMap.DroppedList.Remove(DropId);
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {Amount}", 12));
                            Session.Client.SendPacket(Session.Character.GenerateGold());
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                        }
                    }
                }
            }
        }

        [Packet("#req_exc")]
        public void AcceptExchange(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            byte mode;
            long charId;
            if (!byte.TryParse(packetsplit[2], out mode) || !long.TryParse(packetsplit[3], out charId)) return;

            Session.Character.ExchangeInfo = new ExchangeInfo
            {
                CharId = charId,
                Confirm = false
            };

            if (mode == 2)
            {
                ExchangeInfo exc = ClientLinkManager.Instance.GetProperty<ExchangeInfo>(charId, "ExchangeInfo");
                if (exc == null || exc.ExchangeList.Count() == 0)
                {
                    if (charId == Session.Character.CharacterId) return;
                    Session.Client.SendPacket($"exc_list 1 {charId} -1");
                    ClientLinkManager.Instance.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} -1", ReceiverType.OnlySomeone, "", charId);
                }
                else
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateModal($"refused {Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE")}"), ReceiverType.OnlySomeone, "", charId);
            }
            else if (mode == 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateModal($"refused {Language.Instance.GetMessageFromKey("REFUSED")}"));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateModal($"refused {Language.Instance.GetMessageFromKey("REFUSED")}"), ReceiverType.OnlySomeone, "", charId);
            }
        }

        [Packet("#b_i")]
        public void AnswerToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);

            if (Convert.ToInt32(packetsplit[4]) == 1)
            {
                Session.Client.SendPacket(Session.Character.GenerateDialog($"#b_i^{type}^{slot}^2 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("SURE_TO_DELETE")}"));
            }
            else if (Convert.ToInt32(packetsplit[4]) == 2)
            {
                if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                    return;
                Session.Character.DeleteItem(Session, type, slot);
            }
        }

        [Packet("b_i")]
        public void AskToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            Session.Client.SendPacket(Session.Character.GenerateDialog($"#b_i^{type}^{slot}^1 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("ASK_TO_DELETE")}"));
        }
        [Packet("sl")]
        public void SpTransform(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            Inventory spInventory = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);

            if (packetsplit.Length == 10 && packetsplit[2] == "10")
            {
                // There you go, SP!

                if (!Session.Character.UseSp || spInventory == null || int.Parse(packetsplit[5]) != spInventory.InventoryItem.InventoryItemId)
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPUSE_NEEDED"), 0));
                    return;
                }

                if (ServersData.SpPoint(spInventory.InventoryItem.SpLevel, spInventory.InventoryItem.Upgrade)
                    - spInventory.InventoryItem.SlDamage - spInventory.InventoryItem.SlHP
                    - spInventory.InventoryItem.SlElement - spInventory.InventoryItem.SlDefence
                    - short.Parse(packetsplit[6]) - short.Parse(packetsplit[7])
                    - short.Parse(packetsplit[8]) - short.Parse(packetsplit[9]) < 0)
                    return;

                spInventory.InventoryItem.SlDamage += short.Parse(packetsplit[6]);
                spInventory.InventoryItem.SlDefence += short.Parse(packetsplit[7]);
                spInventory.InventoryItem.SlElement += short.Parse(packetsplit[8]);
                spInventory.InventoryItem.SlHP += short.Parse(packetsplit[9]);

                int slElement = ServersData.SlPoint(spInventory.InventoryItem.SlElement, 2);
                int slHp = ServersData.SlPoint(spInventory.InventoryItem.SlHP, 3);
                int slDefence = ServersData.SlPoint(spInventory.InventoryItem.SlDefence, 1);
                int slHit = ServersData.SlPoint(spInventory.InventoryItem.SlDamage, 0);

                //so add upgrade to sp
                //slhit
                spInventory.InventoryItem.DamageMinimum = 0;
                spInventory.InventoryItem.DamageMaximum = 0;
                spInventory.InventoryItem.HitRate = 0;
                spInventory.InventoryItem.CriticalLuckRate = 0;
                spInventory.InventoryItem.CriticalRate = 0;
                spInventory.InventoryItem.DefenceDodge = 0;
                spInventory.InventoryItem.DistanceDefenceDodge = 0;
                spInventory.InventoryItem.ElementRate = 0;
                spInventory.InventoryItem.DarkResistance = 0;
                spInventory.InventoryItem.LightResistance = 0;
                spInventory.InventoryItem.FireResistance = 0;
                spInventory.InventoryItem.WaterResistance = 0;
                spInventory.InventoryItem.CriticalDodge = 0;
                spInventory.InventoryItem.MagicDefence = 0;
                spInventory.InventoryItem.HP = 0;
                spInventory.InventoryItem.MP = 0;

                if (slHit >= 1)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slHit >= 10)
                {
                    spInventory.InventoryItem.HitRate += 10;
                }
                if (slHit >= 20)
                {
                    spInventory.InventoryItem.CriticalLuckRate += 2;
                }
                if (slHit >= 30)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                    spInventory.InventoryItem.HitRate += 10;
                }
                if (slHit >= 40)
                {
                    spInventory.InventoryItem.CriticalRate += 10;
                }
                if (slHit >= 50)
                {
                    spInventory.InventoryItem.HP += 200;
                    spInventory.InventoryItem.MP += 200;
                }
                if (slHit >= 60)
                {
                    spInventory.InventoryItem.HitRate += 15;
                }
                if (slHit >= 70)
                {
                    spInventory.InventoryItem.HitRate += 15;
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slHit >= 80)
                {
                    spInventory.InventoryItem.CriticalLuckRate += 2;
                }
                if (slHit >= 90)
                {
                    spInventory.InventoryItem.CriticalRate += 20;
                }
                //sldef
                if (slDefence >= 20)
                {
                    spInventory.InventoryItem.DefenceDodge += 2;
                    spInventory.InventoryItem.DistanceDefenceDodge += 2;
                }
                if (slDefence >= 30)
                {
                    spInventory.InventoryItem.HP += 100;
                }
                if (slDefence >= 40)
                {
                    spInventory.InventoryItem.DefenceDodge += 2;
                    spInventory.InventoryItem.DistanceDefenceDodge += 2;
                }
                if (slDefence >= 60)
                {
                    spInventory.InventoryItem.HP += 200;
                }
                if (slDefence >= 70)
                {
                    spInventory.InventoryItem.DefenceDodge += 3;
                    spInventory.InventoryItem.DistanceDefenceDodge += 3;
                }
                if (slDefence >= 75)
                {
                    spInventory.InventoryItem.FireResistance += 2;
                    spInventory.InventoryItem.WaterResistance += 2;
                    spInventory.InventoryItem.LightResistance += 2;
                    spInventory.InventoryItem.DarkResistance += 2;
                }
                if (slDefence >= 80)
                {
                    spInventory.InventoryItem.DefenceDodge += 3;
                    spInventory.InventoryItem.DistanceDefenceDodge += 3;
                }
                if (slDefence >= 90)
                {
                    spInventory.InventoryItem.FireResistance += 3;
                    spInventory.InventoryItem.WaterResistance += 3;
                    spInventory.InventoryItem.LightResistance += 3;
                    spInventory.InventoryItem.DarkResistance += 3;
                }
                if (slDefence >= 95)
                {
                    spInventory.InventoryItem.HP += 300;
                }
                //slele
                if (slElement >= 1)
                {
                    spInventory.InventoryItem.ElementRate += 2;
                }
                if (slElement >= 10)
                {
                    spInventory.InventoryItem.MP += 100;
                }
                if (slElement >= 20)
                {
                    spInventory.InventoryItem.MagicDefence += 5;
                }
                if (slElement >= 30)
                {
                    spInventory.InventoryItem.FireResistance += 2;
                    spInventory.InventoryItem.WaterResistance += 2;
                    spInventory.InventoryItem.LightResistance += 2;
                    spInventory.InventoryItem.DarkResistance += 2;
                    spInventory.InventoryItem.ElementRate += 2;
                }
                if (slElement >= 40)
                {
                    spInventory.InventoryItem.MP += 100;
                }
                if (slElement >= 50)
                {
                    spInventory.InventoryItem.MagicDefence += 5;
                }
                if (slElement >= 60)
                {
                    spInventory.InventoryItem.FireResistance += 3;
                    spInventory.InventoryItem.WaterResistance += 3;
                    spInventory.InventoryItem.LightResistance += 3;
                    spInventory.InventoryItem.DarkResistance += 3;
                    spInventory.InventoryItem.ElementRate += 2;
                }
                if (slElement >= 70)
                {
                    spInventory.InventoryItem.MP += 100;
                }
                if (slElement >= 80)
                {
                    spInventory.InventoryItem.MagicDefence += 5;
                }
                if (slElement >= 90)
                {
                    spInventory.InventoryItem.FireResistance += 4;
                    spInventory.InventoryItem.WaterResistance += 4;
                    spInventory.InventoryItem.LightResistance += 4;
                    spInventory.InventoryItem.DarkResistance += 4;
                }
                if (slElement == 100)
                {
                    spInventory.InventoryItem.FireResistance += 6;
                    spInventory.InventoryItem.WaterResistance += 6;
                    spInventory.InventoryItem.LightResistance += 6;
                    spInventory.InventoryItem.DarkResistance += 6;
                }
                //slhp
                if (slElement >= 5)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 10)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 15)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 20)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                    spInventory.InventoryItem.CloseDefence += 10;
                    spInventory.InventoryItem.DistanceDefence += 10;
                    spInventory.InventoryItem.MagicDefence += 10;
                }
                if (slElement >= 25)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 30)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 35)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 40)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                    spInventory.InventoryItem.CloseDefence += 15;
                    spInventory.InventoryItem.DistanceDefence += 15;
                    spInventory.InventoryItem.MagicDefence += 15;
                }
                if (slElement >= 45)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                }
                if (slElement >= 50)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                    spInventory.InventoryItem.FireResistance += 2;
                    spInventory.InventoryItem.WaterResistance += 2;
                    spInventory.InventoryItem.LightResistance += 2;
                    spInventory.InventoryItem.DarkResistance += 2;
                }
                if (slElement >= 60)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                }
                if (slElement >= 65)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                }
                if (slElement >= 70)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                    spInventory.InventoryItem.CloseDefence += 45;
                    spInventory.InventoryItem.DistanceDefence += 45;
                    spInventory.InventoryItem.MagicDefence += 45;
                }
                if (slElement >= 75)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                }
                if (slElement >= 80)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                }
                if (slElement >= 85)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 86)
                {
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 87)
                {
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 88)
                {
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 90)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                    spInventory.InventoryItem.DefenceDodge += (short)((slElement - 90) * 2);
                    spInventory.InventoryItem.DistanceDefenceDodge += (short)((slElement - 90) * 2);
                }
                if (slElement >= 95)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                }
                if (slElement >= 100)
                {
                    spInventory.InventoryItem.DamageMinimum += 20;
                    spInventory.InventoryItem.DamageMaximum += 20;
                    spInventory.InventoryItem.FireResistance += 3;
                    spInventory.InventoryItem.WaterResistance += 3;
                    spInventory.InventoryItem.LightResistance += 3;
                    spInventory.InventoryItem.DarkResistance += 3;
                    spInventory.InventoryItem.CloseDefence += 30;
                    spInventory.InventoryItem.DistanceDefence += 30;
                    spInventory.InventoryItem.MagicDefence += 30;
                    spInventory.InventoryItem.CriticalDodge += 3;
                }
                Session.Client.SendPacket(Session.Character.GenerateStatChar());
                Session.Client.SendPacket(Session.Character.GenerateStat());
                Session.Client.SendPacket(Session.Character.GenerateSlInfo(new InventoryItem(spInventory.InventoryItem), 2));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_SET"), 0));
            }
            else if (!Session.Character.IsSitting)
            {
                if (spInventory == null)
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    return;
                }

                double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                if (Session.Character.UseSp)
                {
                    Session.Character.LastSp = currentRunningSeconds;
                    new Task(() => RemoveSP(spInventory.InventoryItem.ItemVNum)).Start();
                }
                else
                {
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                    if (timeSpanSinceLastSpUsage >= Session.Character.SpCooldown)
                    {
                        if (Session.Character.ThreadCharChange?.IsAlive == true)
                            Session.Character.ThreadCharChange.Abort();
                        Session.Character.ThreadCharChange = new Thread(ChangeSP);
                        Session.Character.ThreadCharChange.Start();
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                    }
                }
            }
        }



        private async void RemoveSP(short vnum)
        {
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
            Session.Character.Speed -= ServerManager.GetItem(vnum).Speed;
            Session.Character.UseSp = false;

            /* string s2 = "c_info " + chara.name + " - -1 -1 - " + chara.id + " " + ((chara.isGm) ? 2 : 0) + " " + +chara.sex + " " + +chara.Hair.style + " " + +chara.Hair.color + " " + chara.user_class + " " + Stats.GetReput(chara.Reput, chara.dignite.ToString()) + " " + (chara.Sp.inUsing ? chara.Sp.sprite : 0) + " 0 - " + (chara.Sp.inUsing ? chara.Sp.upgrade == 15 ? chara.Sp.wings > 4 ? 0 : 15 : chara.Sp.upgrade : 0) + " " + (chara.Sp.inUsing ? (chara.Sp.wings > 4) ? chara.Sp.wings - 4 : chara.Sp.wings : 0) + " " + (chara.Sp.wings_arena ? 1 : 0);
            chara.Send(s2);
            s2 = "at " + chara.id + " " + chara.MapPoint.map + " " + chara.MapPoint.x + " " + +chara.MapPoint.y + " 2 0 0 1";
            chara.Send(s2); */

            Session.Client.SendPacket(Session.Character.GenerateCond());
            Session.Client.SendPacket(Session.Character.GenerateLev());

            /* string s="sl 0";
               chara.Send(s); */
            Session.Character.SpCooldown = 30;
            foreach (CharacterSkill ski in Session.Character.SkillsSp.Where(s => s.Used))
            {
                short time = ServerManager.GetSkill(ski.SkillVNum).Cooldown;
                double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + time * 100;
                temp /= 1000;
                Session.Character.SpCooldown = temp > Session.Character.SpCooldown ? (int)(temp) : (int)(Session.Character.SpCooldown);
            }

            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), Session.Character.SpCooldown), 11));

            Session.Client.SendPacket($"sd {Session.Character.SpCooldown}");

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);

            /* s="ms_c";
            chara.Send(s); */

            // lev 40 2288403 23 47450 3221180 113500 20086 5
            Session.Client.SendPacket(Session.Character.GenerateSki());
            string[] quicklistpackets = Session.Character.GenerateQuicklist();
            foreach (string quicklist in quicklistpackets)
                Session.Client.SendPacket(quicklist);
            Session.Client.SendPacket(Session.Character.GenerateStat());
            Session.Client.SendPacket(Session.Character.GenerateStatChar());

            await Task.Delay(Session.Character.SpCooldown * 1000);
            if (Session == null || Session.Client == null)
                return;
            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPEAR")), 11));
            Session.Client.SendPacket("sd 0");
        }

        private void ChangeSP()
        {
            Session.Client.SendPacket("delay 5000 3 #sl^1");
            ClientLinkManager.Instance.Broadcast(Session, $"guri 2 1 {Session.Character.CharacterId}", ReceiverType.AllOnMap);
            Thread.Sleep(5000);

            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
            Inventory fairy = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Fairy, (byte)InventoryType.Equipment);
            if (sp == null)
                return;

            if (Session.Character.GetReputIco() < ServerManager.GetItem(sp.InventoryItem.ItemVNum).ReputationMinimum)
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"), 0));
                return;
            }

            if (fairy != null && ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element != ServerManager.GetItem(sp.InventoryItem.ItemVNum).Element && ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element != ServerManager.GetItem(sp.InventoryItem.ItemVNum).SecondaryElement)
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                return;
            }

            Session.Character.UseSp = true;
            Session.Character.Morph = ServerManager.GetItem(sp.InventoryItem.ItemVNum).Morph;
            Session.Character.MorphUpgrade = sp.InventoryItem.Upgrade;
            Session.Character.MorphUpgrade2 = sp.InventoryItem.Design;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);

            // TODO: Send SP Skills here

            /*s = "ski 833 833 833 834 835 836 837 838 839 840 841 21 25 28 37 41 44 49 53 56 340 341 345 352";
            MainFile.maps.SendMap(chara, s, true);
            /*
                qslot 0 1.1.2 1.1.1 1.1.3 0.7.-1 1.1.0 0.7.-1 0.7.-1 0.1.10 1.3.2 1.3.1

                qslot 1 1.1.2 1.1.3 1.1.4 1.1.5 1.1.6 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1

                qslot 2 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1
                */

            // lev 40 2288403 14 72745 3221180 145000 20086 5

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);
            Session.Client.SendPacket(Session.Character.GenerateSpPoint());
            Session.Character.Speed += ServerManager.GetItem(sp.InventoryItem.ItemVNum).Speed;
            Session.Client.SendPacket(Session.Character.GenerateCond());
            Session.Client.SendPacket(Session.Character.GenerateLev());
            Session.Client.SendPacket(Session.Character.GenerateStat());
            Session.Client.SendPacket(Session.Character.GenerateStatChar());
            Session.Character.SkillsSp = new List<CharacterSkill>();
            foreach (Skill ski in ServerManager.GetAllSkill())
            {
                if (ski.Class == Session.Character.Morph + 31)
                    Session.Character.SkillsSp.Add(new CharacterSkill() { SkillVNum = ski.SkillVNum, CharacterId = Session.Character.CharacterId });
            }

            Session.Client.SendPacket(Session.Character.GenerateSki());
            string[] quicklistpackets = Session.Character.GenerateQuicklist();
            foreach (string quicklist in quicklistpackets)
                Session.Client.SendPacket(quicklist);
        }



       

        #endregion
    }
}