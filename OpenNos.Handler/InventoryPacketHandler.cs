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
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

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

        private ClientSession Session => _session;

        #endregion

        #region Methods

        [Packet("#req_exc")]
        public void AcceptExchange(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            byte mode;
            long characterId;
            if (byte.TryParse(packetsplit[2], out mode) && long.TryParse(packetsplit[3], out characterId))
            {
                if (Session.Character.MapInstanceId != ServerManager.Instance.GetProperty<Guid>(characterId, nameof(Character.MapInstanceId)))
                {
                    ServerManager.Instance.SetProperty(characterId, nameof(Character.ExchangeInfo), null);
                    Session.Character.ExchangeInfo = null;
                }
                else
                {
                    switch (mode)
                    {
                        case 2:
                            bool otherInExchangeOrTrade = ServerManager.Instance.GetProperty<bool>(characterId, nameof(Character.InExchangeOrTrade));
                            if (!Session.Character.InExchangeOrTrade || !otherInExchangeOrTrade)
                            {
                                if (characterId == Session.Character.CharacterId || Session.Character.Speed == 0)
                                {
                                    return;
                                }
                                Session.SendPacket($"exc_list 1 {characterId} -1");
                                ExchangeInfo exc = new ExchangeInfo
                                {
                                    TargetCharacterId = characterId,
                                    Confirm = false
                                };
                                Session.Character.ExchangeInfo = exc;
                                ServerManager.Instance.SetProperty(characterId, nameof(Character.ExchangeInfo), new ExchangeInfo { TargetCharacterId = Session.Character.CharacterId, Confirm = false });
                                Session.CurrentMapInstance?.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} -1", ReceiverType.OnlySomeone, string.Empty, characterId);
                            }
                            else
                            {
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0), ReceiverType.OnlySomeone, string.Empty, characterId);
                            }
                            break;

                        case 5:
                            ServerManager.Instance.GetProperty<string>(characterId, nameof(Character.Name));
                            ServerManager.Instance.SetProperty(characterId, nameof(Character.ExchangeInfo), null);
                            Session.Character.ExchangeInfo = null;
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("YOU_REFUSED"), 10));
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EXCHANGE_REFUSED"), Session.Character.Name), 10), ReceiverType.OnlySomeone, string.Empty, characterId);
                            break;
                    }
                }
            }
        }

        [Packet("#b_i")]
        public void AnswerToDelete(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            byte type;
            short slot;
            if (byte.TryParse(packetsplit[2], out type) && short.TryParse(packetsplit[3], out slot))
            {
                switch (Convert.ToInt32(packetsplit[4]))
                {
                    case 1:
                        Session.SendPacket(Session.Character.GenerateDialog($"#b_i^{type}^{slot}^2 #b_i^{type}^{slot}^5 {Language.Instance.GetMessageFromKey("SURE_TO_DELETE")}"));
                        break;

                    case 2:
                        if (Session.Character.InExchangeOrTrade || (InventoryType)type == InventoryType.Bazaar)
                        {
                            return;
                        }
                        Session.Character.DeleteItem((InventoryType)type, slot);
                        break;
                }
            }
        }

        [Packet("b_i")]
        public void AskToDelete(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte type;
            short slot;
            if (byte.TryParse(packetsplit[2], out type) && short.TryParse(packetsplit[3], out slot))
            {
                Session.SendPacket(Session.Character.GenerateDialog($"#b_i^{type}^{slot}^1 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("ASK_TO_DELETE")}"));
            }
        }

        [Packet("s_carrier")]
        public void SpecialistHolder(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short slot;
            short holderSlot;
            if (short.TryParse(packetsplit[2], out slot) && short.TryParse(packetsplit[3], out holderSlot))
            {
                SpecialistInstance specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, InventoryType.Equipment);
                BoxInstance holder = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(holderSlot, InventoryType.Equipment);
                if (specialist != null && holder != null)
                {
                    holder.HoldingVNum = specialist.ItemVNum;
                    holder.SlDamage = specialist.SlDamage;
                    holder.SlDefence = specialist.SlDefence;
                    holder.SlElement = specialist.SlElement;
                    holder.SlHP = specialist.SlHP;
                    holder.SpDamage = specialist.SpDamage;
                    holder.SpDark = specialist.SpDark;
                    holder.SpDefence = specialist.SpDefence;
                    holder.SpElement = specialist.SpElement;
                    holder.SpFire = specialist.SpFire;
                    holder.SpHP = specialist.SpHP;
                    holder.SpLevel = specialist.SpLevel;
                    holder.SpLight = specialist.SpLight;
                    holder.SpStoneUpgrade = specialist.SpStoneUpgrade;
                    holder.SpWater = specialist.SpWater;
                    holder.Upgrade = specialist.Upgrade;
                    holder.XP = specialist.XP;
                    Session.SendPacket("shop_end 2");
                    Session.Character.Inventory.RemoveItemAmountFromInventory(1, specialist.Id);
                }
            }
        }

        [Packet("eqinfo")]
        public void EquipmentInfo(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short slot;
            byte type;
            if (packetsplit.Length > 3 && byte.TryParse(packetsplit[2], out type) && short.TryParse(packetsplit[3], out slot))
            {
                bool isNPCShopItem = false;
                WearableInstance inventory = null;
                switch (type)
                {
                    case 0:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, InventoryType.Wear) ??
                                    Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, InventoryType.Wear);
                        break;

                    case 1:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, InventoryType.Equipment) ??
                                    Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, InventoryType.Equipment) ??
                                    Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(slot, InventoryType.Equipment);
                        break;

                    case 2:
                        isNPCShopItem = true;
                        if (ServerManager.GetItem(slot) != null)
                        {
                            inventory = new WearableInstance(slot, 1);
                            break;
                        }
                        return;

                    case 5:
                        if (Session.Character.ExchangeInfo != null)
                        {
                            if (short.TryParse(packetsplit[3], out slot))
                            {
                                ExchangeInfo exch = ServerManager.Instance.GetProperty<ExchangeInfo>(Session.Character.ExchangeInfo.TargetCharacterId, nameof(Character.ExchangeInfo));
                                if (exch?.ExchangeList?.ElementAtOrDefault(slot) != null)
                                {
                                    Guid id = exch.ExchangeList.ElementAt(slot).Id;
                                    Inventory inv = ServerManager.Instance.GetProperty<Inventory>(Session.Character.ExchangeInfo.TargetCharacterId, nameof(Character.Inventory));
                                    inventory = inv.LoadByItemInstance<WearableInstance>(id) ??
                                                inv.LoadByItemInstance<SpecialistInstance>(id) ??
                                                inv.LoadByItemInstance<BoxInstance>(id);
                                }
                            }
                        }
                        break;

                    case 6:
                        long shopOwnerId;
                        if (long.TryParse(packetsplit[5], out shopOwnerId))
                        {
                            KeyValuePair<long, MapShop> shop = Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(shopOwnerId));
                            PersonalShopItem item = shop.Value?.Items.FirstOrDefault(i => i.ShopSlot.Equals(slot));
                            if (item != null)
                            {
                                if (item.ItemInstance.GetType() == typeof(BoxInstance))
                                {
                                    inventory = (BoxInstance)item.ItemInstance;
                                }
                                else
                                {
                                    try
                                    {
                                        inventory = (WearableInstance)item.ItemInstance;
                                    }
                                    catch
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        break;

                    case 7:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, InventoryType.Specialist); // Partner inv
                        break;

                    case 10:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, InventoryType.Specialist);
                        break;

                    case 11:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, InventoryType.Costume);
                        break;
                }
                if (inventory?.Item != null)
                {
                    if (inventory.IsEmpty || isNPCShopItem)
                    {
                        Session.SendPacket(Session.Character.GenerateEInfo(inventory));
                        return;
                    }
                    Session.SendPacket(inventory.Item.EquipmentSlot != EquipmentType.Sp ?
                        Session.Character.GenerateEInfo(inventory) : inventory.Item.SpType == 0 && inventory.Item.ItemSubType == 4 ?
                        Session.Character.GeneratePslInfo(inventory as SpecialistInstance) : Session.Character.GenerateSlInfo(inventory as SpecialistInstance, 0));
                }
            }
        }

        [Packet("exc_list")]
        public void ExchangeList(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            long gold;
            long.TryParse(packetsplit[2], out gold);
            byte[] type = new byte[10], qty = new byte[10];
            short[] slot = new short[10];
            string packetList = string.Empty;

            if (gold < 0 || gold > Session.Character.Gold || Session.Character.ExchangeInfo == null || Session.Character.ExchangeInfo.ExchangeList.Any())
            {
                return;
            }

            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);
            if (Session.Character.HasShopOpened || targetSession != null && targetSession.Character.HasShopOpened)
            {
                CloseExchange(Session, targetSession);
                return;
            }

            for (int j = 6, i = 0; j <= packetsplit.Length; j += 3, i++)
            {
                byte.TryParse(packetsplit[j - 3], out type[i]);
                short.TryParse(packetsplit[j - 2], out slot[i]);
                byte.TryParse(packetsplit[j - 1], out qty[i]);
                if ((InventoryType)type[i] == InventoryType.Bazaar)
                {
                    CloseExchange(Session, targetSession);
                    return;
                }
                ItemInstance item = Session.Character.Inventory.LoadBySlotAndType(slot[i], (InventoryType)type[i]);
                if (item == null)
                {
                    return;
                }
                if (qty[i] <= 0 || item.Amount < qty[i])
                {
                    return;
                }
                ItemInstance it = item.DeepCopy();
                if (it.Item.IsTradable && !it.IsBound)
                {
                    it.Amount = qty[i];
                    Session.Character.ExchangeInfo.ExchangeList.Add(it);
                    if (type[i] != 0)
                    {
                        packetList += $"{i}.{type[i]}.{it.ItemVNum}.{qty[i]} ";
                    }
                    else
                    {
                        packetList += $"{i}.{type[i]}.{it.ItemVNum}.{it.Rare}.{it.Upgrade} ";
                    }
                }
                else if (it.IsBound)
                {
                    Session.SendPacket("exc_close 0");
                    Session.CurrentMapInstance?.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, string.Empty, Session.Character.ExchangeInfo.TargetCharacterId);

                    ServerManager.Instance.SetProperty(Session.Character.ExchangeInfo.TargetCharacterId, nameof(Character.ExchangeInfo), null);
                    Session.Character.ExchangeInfo = null;
                    return;
                }
            }
            Session.Character.ExchangeInfo.Gold = gold;
            Session.CurrentMapInstance?.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} {gold} {packetList}", ReceiverType.OnlySomeone, string.Empty, Session.Character.ExchangeInfo.TargetCharacterId);
            Session.Character.ExchangeInfo.Validate = true;
        }

        [Packet("req_exc")]
        public void ExchangeRequest(string deserializedPacket)
        {
            Logger.Debug(deserializedPacket, Session.SessionId);
            ExchangeRequestPacket packet = PacketFactory.Deserialize<ExchangeRequestPacket>(deserializedPacket, true);
            if (packet != null)
            {
                switch (packet.RequestType)
                {
                    case RequestExchangeType.Requested: // send the request trade
                        {
                            if (!Session.HasCurrentMapInstance)
                            {
                                return;
                            }
                            ClientSession targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(packet.CharacterId);

                            if (targetSession == null)
                            {
                                return;
                            }

                            if (Session.Character.IsBlockedByCharacter(packet.CharacterId))
                            {
                                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                                return;
                            }


                            if (Session.Character.Speed == 0 || targetSession.Character.Speed == 0)
                            {
                                Session.Character.ExchangeBlocked = true;
                            }
                            if (targetSession.Character.LastSkillUse.AddSeconds(20) > DateTime.Now || targetSession.Character.LastDefence.AddSeconds(20) > DateTime.Now)
                            {
                                Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("PLAYER_IN_BATTLE"), targetSession.Character.Name)));
                                return;
                            }

                            if (Session.Character.LastSkillUse.AddSeconds(20) > DateTime.Now || Session.Character.LastDefence.AddSeconds(20) > DateTime.Now)
                            {
                                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("IN_BATTLE")));
                                return;
                            }

                            if (Session.Character.HasShopOpened || targetSession.Character.HasShopOpened)
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HAS_SHOP_OPENED"), 10));
                                return;
                            }

                            if (targetSession.Character.ExchangeBlocked)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRADE_BLOCKED"), 11));
                            }
                            else
                            {
                                if (Session.Character.InExchangeOrTrade || targetSession.Character.InExchangeOrTrade)
                                {
                                    Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0));
                                }
                                else
                                {
                                    Session.SendPacket(Session.Character.GenerateModal(string.Format(Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE"), targetSession.Character.Name), 0));
                                    targetSession.SendPacket(Session.Character.GenerateDialog($"#req_exc^2^{Session.Character.CharacterId} #req_exc^5^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INCOMING_EXCHANGE"), Session.Character.Name)}"));
                                }
                            }
                            break;
                        }
                    case RequestExchangeType.Confirmed: // click Trade button in exchange window
                        {
                            if (Session.HasCurrentMapInstance && Session.HasSelectedCharacter
                                && Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.TargetCharacterId != Session.Character.CharacterId)
                            {
                                if (!Session.HasCurrentMapInstance)
                                {
                                    return;
                                }
                                ClientSession targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);

                                if (targetSession == null)
                                {
                                    return;
                                }

                                if (Session.IsDisposing || targetSession.IsDisposing)
                                {
                                    CloseExchange(Session, targetSession);
                                    return;
                                }

                                lock (targetSession.Character.Inventory)
                                {
                                    lock (Session.Character.Inventory)
                                    {
                                        ExchangeInfo targetExchange = targetSession.Character.ExchangeInfo;
                                        Inventory inventory = targetSession.Character.Inventory;

                                        long gold = targetSession.Character.Gold;
                                        int backpack = targetSession.Character.HaveBackpack() ? 1 : 0;

                                        if (targetExchange == null)
                                        {
                                            return;
                                        }
                                        if (Session.Character.ExchangeInfo.Validate && targetExchange.Validate)
                                        {
                                            Session.Character.ExchangeInfo.Confirm = true;
                                            if (targetExchange.Confirm && Session.Character.ExchangeInfo.Confirm)
                                            {
                                                Session.SendPacket("exc_close 1");
                                                targetSession.SendPacket("exc_close 1");

                                                bool @continue = true;
                                                bool goldmax = false;
                                                if (!Session.Character.Inventory.GetFreeSlotAmount(targetExchange.ExchangeList, Session.Character.HaveBackpack() ? 1 : 0))
                                                {
                                                    @continue = false;
                                                }
                                                if (!inventory.GetFreeSlotAmount(Session.Character.ExchangeInfo.ExchangeList, backpack))
                                                {
                                                    @continue = false;
                                                }
                                                if (Session.Character.ExchangeInfo.Gold + gold > 1000000000)
                                                {
                                                    goldmax = true;
                                                }
                                                if (Session.Character.ExchangeInfo.Gold > Session.Character.Gold)
                                                {
                                                    return;
                                                }
                                                if (targetExchange.Gold + Session.Character.Gold > 1000000000)
                                                {
                                                    goldmax = true;
                                                }
                                                if (!@continue || goldmax)
                                                {
                                                    string message = !@continue ? Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0)
                                                        : Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                                                    Session.SendPacket(message);
                                                    targetSession.SendPacket(message);
                                                    CloseExchange(Session, targetSession);
                                                }
                                                else
                                                {
                                                    if (Session.Character.ExchangeInfo.ExchangeList.Any(ei => !(ei.Item.IsTradable || ei.IsBound)))
                                                    {
                                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_TRADABLE"), 0));
                                                        CloseExchange(Session, targetSession);
                                                    }
                                                    else // all items can be traded
                                                    {
                                                        Session.Character.IsExchanging = targetSession.Character.IsExchanging = true;

                                                        // exchange all items from target to source
                                                        Exchange(targetSession, Session);

                                                        // exchange all items from source to target
                                                        Exchange(Session, targetSession);

                                                        Session.Character.IsExchanging = targetSession.Character.IsExchanging = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("IN_WAITING_FOR"), targetSession.Character.Name)));
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    case RequestExchangeType.Cancelled: // cancel trade thru exchange window
                        {
                            if (Session.HasCurrentMapInstance && Session.Character.ExchangeInfo != null)
                            {
                                ClientSession targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);
                                CloseExchange(Session, targetSession);
                            }
                            break;
                        }
                    default:
                        Logger.Log.Warn($"Exchange-Request-Type not implemented. RequestType: {packet.RequestType})");
                        break;
                }
            }
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="packet"></param>
        public void GetItem(GetPacket packet)
        {
            Logger.Debug(packet.ToString(), Session.SessionId);

            if (!Session.HasCurrentMapInstance || !Session.CurrentMapInstance.DroppedList.ContainsKey(packet.TransportId))
            {
                return;
            }

            MapItem mapItem = Session.CurrentMapInstance.DroppedList[packet.TransportId];
            if (Session.Character.LastSkillUse.AddSeconds(1) > DateTime.Now || Session.Character.IsVehicled)
            {
                return;
            }
            if (mapItem != null)
            {
                if (Session.Character.IsInRange(mapItem.PositionX, mapItem.PositionY, 8) && Session.HasCurrentMapInstance)
                {
                    MonsterMapItem item = mapItem as MonsterMapItem;
                    if (item != null)
                    {
                        MonsterMapItem monsterMapItem = item;
                        if (monsterMapItem.OwnerId.HasValue)
                        {
                            Group group = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(monsterMapItem.OwnerId.Value) && g.IsMemberOfGroup(Session.Character.CharacterId));
                            if (item.CreatedDate.AddSeconds(30) > DateTime.Now && !(monsterMapItem.OwnerId == Session.Character.CharacterId || group != null && group.SharingMode == (byte)GroupSharingType.Everyone))
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_YOUR_ITEM"), 10));
                                return;
                            }
                        }

                        // initialize and rarify
                        item.Rarify(null);
                    }

                    if (mapItem.ItemVNum != 1046)
                    {
                        ItemInstance mapItemInstance = mapItem.GetItemInstance();
                        if (mapItemInstance.Item.ItemType == ItemType.Map)
                        {
                            if (mapItemInstance.Item.Effect == 71)
                            {
                                Session.Character.SpPoint += mapItem.GetItemInstance().Item.EffectValue;
                                if (Session.Character.SpPoint > 10000)
                                {
                                    Session.Character.SpPoint = 10000;
                                }
                                Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"), mapItem.GetItemInstance().Item.EffectValue), 0));
                                Session.SendPacket(Session.Character.GenerateSpPoint());
                            }
                            Session.CurrentMapInstance.DroppedList.Remove(packet.TransportId);
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGet(packet.TransportId));
                        }
                        else
                        {
                            lock (Session.Character.Inventory)
                            {
                                ItemInstance newInv = Session.Character.Inventory.AddToInventory(mapItemInstance);
                                if (newInv != null)
                                {
                                    Session.CurrentMapInstance.DroppedList.Remove(packet.TransportId);
                                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGet(packet.TransportId));
                                    Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                    Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newInv.Item.Name} x {mapItem.Amount}", 12));
                                }
                                else
                                {
                                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }
                    else
                    {
                        // handle gold drop
                        MonsterMapItem droppedGold = mapItem as MonsterMapItem;
                        if (droppedGold != null && Session.Character.Gold + droppedGold.GoldAmount <= 1000000000)
                        {
                            Session.Character.Gold += droppedGold.GoldAmount;
                            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {mapItem.GetItemInstance().Item.Name} x {droppedGold.GoldAmount}", 12));
                        }
                        else
                        {
                            Session.Character.Gold = 1000000000;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                        }
                        Session.SendPacket(Session.Character.GenerateGold());
                        Session.CurrentMapInstance.DroppedList.Remove(packet.TransportId);
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGet(packet.TransportId));
                    }
                }
            }
        }

        /// <summary>
        /// mve
        /// </summary>
        /// <param name="packet"></param>
        public void MoveEquipment(MvePacket packet)
        {
            Logger.Debug(packet.ToString(), Session.SessionId);
            lock (Session.Character.Inventory)
            {
                if (packet.DestinationSlot > 48 + (Session.Character.HaveBackpack() ? 1 : 0) * 12)
                {
                    return;
                }
                if (Session.Character.InExchangeOrTrade)
                {
                    return;
                }
                ItemInstance sourceItem = Session.Character.Inventory.LoadBySlotAndType(packet.Slot, packet.InventoryType);
                if (sourceItem != null && sourceItem.Item.ItemType == ItemType.Specialist || sourceItem != null && sourceItem.Item.ItemType == ItemType.Fashion)
                {
                    ItemInstance inv = Session.Character.Inventory.MoveInInventory(packet.Slot, packet.InventoryType, packet.DestinationInventoryType, packet.DestinationSlot, false);
                    if (inv != null)
                    {
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, packet.DestinationInventoryType, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, packet.InventoryType, packet.Slot, 0, 0, 0, 0));
                    }
                }
            }
        }

        /// <summary>
        /// mvi
        /// </summary>
        /// <param name="packet"></param>
        public void MoveItem(MviPacket packet)
        {
            Logger.Debug(packet.ToString(), Session.SessionId);
            lock (Session.Character.Inventory)
            {
                ItemInstance previousInventory;
                ItemInstance newInventory;

                // check if the destination slot is out of range
                if (packet.DestinationSlot > 48 + (Session.Character.HaveBackpack() ? 1 : 0) * 12)
                {
                    return;
                }

                // check if the character is allowed to move the item
                if (Session.Character.InExchangeOrTrade)
                {
                    return;
                }

                // actually move the item from source to destination
                Session.Character.Inventory.MoveItem(packet.InventoryType, packet.Slot, packet.Amount, packet.DestinationSlot, out previousInventory, out newInventory);
                if (newInventory == null)
                {
                    return;
                }
                Session.SendPacket(Session.Character.GenerateInventoryAdd(newInventory.ItemVNum, newInventory.Amount, packet.InventoryType, newInventory.Slot, newInventory.Rare, newInventory.Design, newInventory.Upgrade, 0));

                Session.SendPacket(previousInventory != null
                    ? Session.Character.GenerateInventoryAdd(previousInventory.ItemVNum, previousInventory.Amount,
                        packet.InventoryType, previousInventory.Slot, previousInventory.Rare, previousInventory.Design,
                        previousInventory.Upgrade, 0)
                    : Session.Character.GenerateInventoryAdd(-1, 0, packet.InventoryType, packet.Slot, 0, 0, 0, 0));
            }
        }

        /// <summary>
        /// put packet
        /// </summary>
        /// <param name="putPacket"></param>
        public void PutItem(PutPacket putPacket)
        {
            Logger.Debug(putPacket.ToString(), Session.SessionId);
            lock (Session.Character.Inventory)
            {
                ItemInstance invitem = Session.Character.Inventory.LoadBySlotAndType(putPacket.Slot, putPacket.InventoryType);
                if (invitem != null && invitem.Item.IsDroppable && invitem.Item.IsTradable && !Session.Character.InExchangeOrTrade && putPacket.InventoryType != InventoryType.Bazaar)
                {
                    if (putPacket.Amount > 0 && putPacket.Amount < 100)
                    {
                        if (Session.Character.MapInstance.DroppedList.GetAllItems().Count < 200 && Session.HasCurrentMapInstance)
                        {
                            MapItem droppedItem = Session.CurrentMapInstance.PutItem(putPacket.InventoryType, putPacket.Slot, putPacket.Amount, ref invitem, Session);
                            if (droppedItem == null)
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE_HERE"), 0));
                                return;
                            }
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(invitem.ItemVNum, invitem.Amount, putPacket.InventoryType, invitem.Slot, invitem.Rare, invitem.Design, invitem.Upgrade, 0));

                            if (invitem.Amount == 0)
                            {
                                Session.Character.DeleteItem(invitem.Type, invitem.Slot);
                            }
                            Session.CurrentMapInstance?.Broadcast($"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.Amount} 0 -1");
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_MAP_FULL"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DROP_AMOUNT"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
                }
            }
        }

        [Packet("remove")]
        public void Remove(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            short slot;
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3 && Session.HasCurrentMapInstance && Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null && (Session.Character.ExchangeInfo == null || !(Session.Character.ExchangeInfo?.ExchangeList).Any()) && short.TryParse(packetsplit[2], out slot))
            {
                ItemInstance inventory = slot != (byte)EquipmentType.Sp ? Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, InventoryType.Wear) : Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, InventoryType.Wear);
                if (inventory != null)
                {
                    double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                    if (slot == (byte)EquipmentType.Sp && Session.Character.UseSp)
                    {
                        if (Session.Character.IsVehicled)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                            return;
                        }
                        if (Session.Character.LastSkillUse.AddSeconds(2) > DateTime.Now)
                        {
                            return;
                        }
                        Session.Character.LastSp = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                        RemoveSP(inventory.ItemVNum);
                    }
                    else if (slot == (byte)EquipmentType.Sp && !Session.Character.UseSp && timeSpanSinceLastSpUsage <= Session.Character.SpCooldown)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                        return;
                    }

                    ItemInstance inv = Session.Character.Inventory.MoveInInventory(slot, InventoryType.Wear, InventoryType.Equipment);

                    if (inv == null)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        return;
                    }

                    if (inv.Slot != -1)
                    {
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(inventory.ItemVNum, inv.Amount, inv.Type, inv.Slot, inventory.Rare, inventory.Design, inventory.Upgrade, 0));
                    }

                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEq());
                    Session.SendPacket(Session.Character.GenerateEquipment());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GeneratePairy());
                }
            }
        }

        [Packet("#sl")]
        public void Sl(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            byte mode;
            if (byte.TryParse(packetsplit[2], out mode) && !Session.Character.UseSp && !Session.Character.IsVehicled)
            {
                double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                if (timeSpanSinceLastSpUsage >= Session.Character.SpCooldown)
                {
                    ChangeSP();
                }
            }
        }

        [Packet("sortopen")]
        public void SortOpen(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            bool gravity = true;
            while (gravity)
            {
                gravity = false;
                for (short i = 0; i < 2; i++)
                {
                    for (short x = 0; x < 44; x++)
                    {
                        InventoryType type = i == 0 ? InventoryType.Specialist : InventoryType.Costume;
                        if (Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(x, type) == null)
                        {
                            if (Session.Character.Inventory.LoadBySlotAndType<ItemInstance>((short)(x + 1), type) != null)
                            {
                                ItemInstance invdest;
                                ItemInstance inv;
                                Session.Character.Inventory.MoveItem(type, (short)(x + 1), 1, x, out inv, out invdest);
                                WearableInstance wearableInstance = invdest as WearableInstance;
                                if (wearableInstance != null)
                                {
                                    Session.SendPacket(Session.Character.GenerateInventoryAdd(invdest.ItemVNum, invdest.Amount, type, invdest.Slot, wearableInstance.Rare, wearableInstance.Design, wearableInstance.Upgrade, 0));
                                }
                                Session.Character.DeleteItem(type, (short)(x + 1));
                                gravity = true;
                            }
                        }
                    }
                    Session.Character.Inventory.Reorder(Session, i == 0 ? InventoryType.Specialist : InventoryType.Costume);
                }
            }
        }

        [Packet("#u_i")]
        public void SpecialUseItem(string packet)
        {
            UseItem(packet);
        }

        [Packet("sl")]
        public void SpTransform(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');

            SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);

            if (packetsplit.Length == 10 && packetsplit[2] == "10")
            {
                short specialistDamage, specialistDefense, specialistElement, specialistHealpoints;
                int transportId;
                if (!int.TryParse(packetsplit[5], out transportId) || !short.TryParse(packetsplit[6], out specialistDamage) || !short.TryParse(packetsplit[7], out specialistDefense) || !short.TryParse(packetsplit[8], out specialistElement) || !short.TryParse(packetsplit[9], out specialistHealpoints))
                {
                    return;
                }
                if (!Session.Character.UseSp || specialistInstance == null || transportId != specialistInstance.TransportId)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPUSE_NEEDED"), 0));
                    return;
                }
                if (CharacterHelper.SPPoint(specialistInstance.SpLevel, specialistInstance.Upgrade) - specialistInstance.SlDamage - specialistInstance.SlHP - specialistInstance.SlElement - specialistInstance.SlDefence - specialistDamage - specialistDefense - specialistElement - specialistHealpoints < 0)
                {
                    return;
                }
                if (specialistDamage < 0 || specialistDefense < 0 || specialistElement < 0 || specialistHealpoints < 0)
                {
                    return;
                }

                specialistInstance.SlDamage += specialistDamage;
                specialistInstance.SlDefence += specialistDefense;
                specialistInstance.SlElement += specialistElement;
                specialistInstance.SlHP += specialistHealpoints;

                int slElement = CharacterHelper.SlPoint(specialistInstance.SlElement, 2);
                int slHp = CharacterHelper.SlPoint(specialistInstance.SlHP, 3);
                int slDefence = CharacterHelper.SlPoint(specialistInstance.SlDefence, 1);
                int slHit = CharacterHelper.SlPoint(specialistInstance.SlDamage, 0);

                #region slHit

                specialistInstance.DamageMinimum = 0;
                specialistInstance.DamageMaximum = 0;
                specialistInstance.HitRate = 0;
                specialistInstance.CriticalLuckRate = 0;
                specialistInstance.CriticalRate = 0;
                specialistInstance.DefenceDodge = 0;
                specialistInstance.DistanceDefenceDodge = 0;
                specialistInstance.ElementRate = 0;
                specialistInstance.DarkResistance = 0;
                specialistInstance.LightResistance = 0;
                specialistInstance.FireResistance = 0;
                specialistInstance.WaterResistance = 0;
                specialistInstance.CriticalDodge = 0;
                specialistInstance.CloseDefence = 0;
                specialistInstance.DistanceDefence = 0;
                specialistInstance.MagicDefence = 0;
                specialistInstance.HP = 0;
                specialistInstance.MP = 0;

                if (slHit >= 1)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHit >= 10)
                {
                    specialistInstance.HitRate += 10;
                }
                if (slHit >= 20)
                {
                    specialistInstance.CriticalLuckRate += 2;
                }
                if (slHit >= 30)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.HitRate += 10;
                }
                if (slHit >= 40)
                {
                    specialistInstance.CriticalRate += 10;
                }
                if (slHit >= 50)
                {
                    specialistInstance.HP += 200;
                    specialistInstance.MP += 200;
                }
                if (slHit >= 60)
                {
                    specialistInstance.HitRate += 15;
                }
                if (slHit >= 70)
                {
                    specialistInstance.HitRate += 15;
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHit >= 80)
                {
                    specialistInstance.CriticalLuckRate += 3;
                }
                if (slHit >= 90)
                {
                    specialistInstance.CriticalRate += 20;
                }
                if (slHit >= 100)
                {
                    specialistInstance.CriticalLuckRate += 3;
                    specialistInstance.CriticalRate += 20;
                    specialistInstance.HP += 200;
                    specialistInstance.MP += 200;
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.HitRate += 20;
                }

                #endregion

                #region slDefence

                if (slDefence >= 10)
                {
                    specialistInstance.DefenceDodge += 5;
                    specialistInstance.DistanceDefenceDodge += 5;
                }
                if (slDefence >= 20)
                {
                    specialistInstance.CriticalDodge += 2;
                }
                if (slDefence >= 30)
                {
                    specialistInstance.HP += 100;
                }
                if (slDefence >= 40)
                {
                    specialistInstance.CriticalDodge += 2;
                }
                if (slDefence >= 50)
                {
                    specialistInstance.DefenceDodge += 5;
                    specialistInstance.DistanceDefenceDodge += 5;
                }
                if (slDefence >= 60)
                {
                    specialistInstance.HP += 200;
                }
                if (slDefence >= 70)
                {
                    specialistInstance.CriticalDodge += 3;
                }
                if (slDefence >= 75)
                {
                    specialistInstance.FireResistance += 2;
                    specialistInstance.WaterResistance += 2;
                    specialistInstance.LightResistance += 2;
                    specialistInstance.DarkResistance += 2;
                }
                if (slDefence >= 80)
                {
                    specialistInstance.DefenceDodge += 10;
                    specialistInstance.DistanceDefenceDodge += 10;
                    specialistInstance.CriticalDodge += 3;
                }
                if (slDefence >= 90)
                {
                    specialistInstance.FireResistance += 3;
                    specialistInstance.WaterResistance += 3;
                    specialistInstance.LightResistance += 3;
                    specialistInstance.DarkResistance += 3;
                }
                if (slDefence >= 95)
                {
                    specialistInstance.HP += 300;
                }
                if (slDefence >= 100)
                {
                    specialistInstance.DefenceDodge += 20;
                    specialistInstance.DistanceDefenceDodge += 20;
                    specialistInstance.FireResistance += 5;
                    specialistInstance.WaterResistance += 5;
                    specialistInstance.LightResistance += 5;
                    specialistInstance.DarkResistance += 5;
                }

                #endregion

                #region slHp

                if (slHp >= 5)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHp >= 10)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHp >= 15)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHp >= 20)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.CloseDefence += 10;
                    specialistInstance.DistanceDefence += 10;
                    specialistInstance.MagicDefence += 10;
                }
                if (slHp >= 25)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHp >= 30)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHp >= 35)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                }
                if (slHp >= 40)
                {
                    specialistInstance.DamageMinimum += 5;
                    specialistInstance.DamageMaximum += 5;
                    specialistInstance.CloseDefence += 15;
                    specialistInstance.DistanceDefence += 15;
                    specialistInstance.MagicDefence += 15;
                }
                if (slHp >= 45)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }
                if (slHp >= 50)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                    specialistInstance.FireResistance += 2;
                    specialistInstance.WaterResistance += 2;
                    specialistInstance.LightResistance += 2;
                    specialistInstance.DarkResistance += 2;
                }
                if (slHp >= 55)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }
                if (slHp >= 60)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }
                if (slHp >= 65)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                }
                if (slHp >= 70)
                {
                    specialistInstance.DamageMinimum += 10;
                    specialistInstance.DamageMaximum += 10;
                    specialistInstance.CloseDefence += 20;
                    specialistInstance.DistanceDefence += 20;
                    specialistInstance.MagicDefence += 20;
                }
                if (slHp >= 75)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                }
                if (slHp >= 80)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                }
                if (slHp >= 85)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                    specialistInstance.CriticalDodge += 1;
                }
                if (slHp >= 86)
                {
                    specialistInstance.CriticalDodge += 1;
                }
                if (slHp >= 87)
                {
                    specialistInstance.CriticalDodge += 1;
                }
                if (slHp >= 88)
                {
                    specialistInstance.CriticalDodge += 1;
                }
                if (slHp >= 90)
                {
                    specialistInstance.DamageMinimum += 15;
                    specialistInstance.DamageMaximum += 15;
                    specialistInstance.CloseDefence += 25;
                    specialistInstance.DistanceDefence += 25;
                    specialistInstance.MagicDefence += 25;
                }
                if (slHp >= 91)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 92)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 93)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 94)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 95)
                {
                    specialistInstance.DamageMinimum += 20;
                    specialistInstance.DamageMaximum += 20;
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 96)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 97)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 98)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 99)
                {
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                }
                if (slHp >= 100)
                {
                    specialistInstance.FireResistance += 3;
                    specialistInstance.WaterResistance += 3;
                    specialistInstance.LightResistance += 3;
                    specialistInstance.DarkResistance += 3;
                    specialistInstance.CloseDefence += 30;
                    specialistInstance.DistanceDefence += 30;
                    specialistInstance.MagicDefence += 30;
                    specialistInstance.DamageMinimum += 20;
                    specialistInstance.DamageMaximum += 20;
                    specialistInstance.DefenceDodge += 2;
                    specialistInstance.DistanceDefenceDodge += 2;
                    specialistInstance.CriticalDodge += 1;
                }

                #endregion

                #region slElement

                if (slElement >= 1)
                {
                    specialistInstance.ElementRate += 2;
                }
                if (slElement >= 10)
                {
                    specialistInstance.MP += 100;
                }
                if (slElement >= 20)
                {
                    specialistInstance.MagicDefence += 5;
                }
                if (slElement >= 30)
                {
                    specialistInstance.FireResistance += 2;
                    specialistInstance.WaterResistance += 2;
                    specialistInstance.LightResistance += 2;
                    specialistInstance.DarkResistance += 2;
                    specialistInstance.ElementRate += 2;
                }
                if (slElement >= 40)
                {
                    specialistInstance.MP += 100;
                }
                if (slElement >= 50)
                {
                    specialistInstance.MagicDefence += 5;
                }
                if (slElement >= 60)
                {
                    specialistInstance.FireResistance += 3;
                    specialistInstance.WaterResistance += 3;
                    specialistInstance.LightResistance += 3;
                    specialistInstance.DarkResistance += 3;
                    specialistInstance.ElementRate += 2;
                }
                if (slElement >= 70)
                {
                    specialistInstance.MP += 100;
                }
                if (slElement >= 80)
                {
                    specialistInstance.MagicDefence += 5;
                }
                if (slElement >= 90)
                {
                    specialistInstance.FireResistance += 4;
                    specialistInstance.WaterResistance += 4;
                    specialistInstance.LightResistance += 4;
                    specialistInstance.DarkResistance += 4;
                    specialistInstance.ElementRate += 2;
                }
                if (slElement >= 100)
                {
                    specialistInstance.FireResistance += 6;
                    specialistInstance.WaterResistance += 6;
                    specialistInstance.LightResistance += 6;
                    specialistInstance.DarkResistance += 6;
                    specialistInstance.MagicDefence += 5;
                    specialistInstance.MP += 200;
                    specialistInstance.ElementRate += 2;
                }

                #endregion

                Session.SendPacket(Session.Character.GenerateStatChar());
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateSlInfo(specialistInstance, 2));
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_SET"), 0));
            }
            else if (!Session.Character.IsSitting)
            {
                if (Session.Character.Skills.GetAllItems().Any(s => !s.CanBeUsed()))
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILLS_IN_LOADING"), 0));
                    return;
                }
                if (specialistInstance == null)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    return;
                }
                if (Session.Character.IsVehicled)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                    return;
                }

                double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                if (Session.Character.UseSp)
                {
                    Session.Character.LastSp = currentRunningSeconds;
                    RemoveSP(specialistInstance.ItemVNum);
                }
                else
                {
                    if (Session.Character.LastMove.AddSeconds(1) >= DateTime.Now || Session.Character.LastSkillUse.AddSeconds(2) >= DateTime.Now)
                    {
                        return;
                    }
                    if (Session.Character.SpPoint == 0 && Session.Character.SpAdditionPoint == 0)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SP_NOPOINTS"), 0));
                    }
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                    if (timeSpanSinceLastSpUsage >= Session.Character.SpCooldown)
                    {
                        Session.SendPacket(Session.Character.GenerateDelay(5000, 3, "#sl^1"));
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGuri(2, 1), Session.Character.PositionX, Session.Character.PositionY);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                    }
                }
            }
        }

        [Packet("up_gr")]
        public void Upgrade(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.Speed == 0)
            {
                return;
            }
            if (packetsplit.Length > 4)
            {
                InventoryType inventoryType, type2 = 0;
                byte uptype, slot, slot2 = 0;
                byte.TryParse(packetsplit[2], out uptype);
                Enum.TryParse(packetsplit[3], out inventoryType);
                byte.TryParse(packetsplit[4], out slot);

                if (packetsplit.Length > 6)
                {
                    Enum.TryParse(packetsplit[5], out type2);
                    byte.TryParse(packetsplit[6], out slot2);
                }
                WearableInstance inventory;
                switch (uptype)
                {
                    case 1:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, inventoryType);
                        if (inventory != null)
                        {
                            if (inventory.Item.EquipmentSlot == EquipmentType.Armor || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            {
                                inventory.UpgradeItem(Session, UpgradeMode.Normal, UpgradeProtection.None);
                            }
                        }
                        break;

                    case 7:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, inventoryType);
                        if (inventory != null)
                        {
                            if (inventory.Item.EquipmentSlot == EquipmentType.Armor || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            {
                                inventory.RarifyItem(Session, RarifyMode.Normal, RarifyProtection.None);
                            }
                            Session.SendPacket("shop_end 1");
                        }
                        break;

                    case 8:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, inventoryType);
                        WearableInstance inventory2 = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot2, type2);

                        if (inventory != null && inventory2 != null && !Equals(inventory, inventory2))
                        {
                            inventory.Sum(Session, inventory2);
                        }
                        break;

                    case 9:
                        SpecialistInstance specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, inventoryType);
                        if (specialist != null)
                        {
                            if (specialist.Rare != -2)
                            {
                                if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                                {
                                    specialist.UpgradeSp(Session, UpgradeProtection.None);
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                            }
                        }
                        break;

                    case 20:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, inventoryType);
                        if (inventory != null)
                        {
                            if (inventory.Item.EquipmentSlot == EquipmentType.Armor || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            {
                                inventory.UpgradeItem(Session, UpgradeMode.Normal, UpgradeProtection.Protected);
                            }
                        }
                        break;

                    case 21:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, inventoryType);
                        if (inventory != null)
                        {
                            if (inventory.Item.EquipmentSlot == EquipmentType.Armor || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            {
                                inventory.RarifyItem(Session, RarifyMode.Normal, RarifyProtection.Scroll);
                            }
                        }
                        break;

                    case 25:
                        specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, inventoryType);
                        if (specialist != null)
                        {
                            if (specialist.Rare != -2)
                            {
                                if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                                {
                                    specialist.UpgradeSp(Session, UpgradeProtection.Protected);
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                            }
                        }
                        break;

                    case 26:
                        specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, inventoryType);
                        if (specialist != null)
                        {
                            if (specialist.Rare != -2)
                            {
                                if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                                {
                                    specialist.UpgradeSp(Session, UpgradeProtection.Protected);
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                            }
                        }
                        break;

                    case 41:
                        specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(slot, inventoryType);
                        if (specialist != null)
                        {
                            if (specialist.Rare != -2)
                            {
                                if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                                {
                                    specialist.PerfectSP(Session);
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                            }
                        }
                        break;

                    case 43:
                        inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(slot, inventoryType);
                        if (inventory != null)
                        {
                            if (inventory.Item.EquipmentSlot == EquipmentType.Armor || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            {
                                inventory.UpgradeItem(Session, UpgradeMode.Reduced, UpgradeProtection.Protected);
                            }
                        }
                        break;
                }
            }
        }

        [Packet("u_i")]
        public void UseItem(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            short slot;
            byte type;
            if (packetsplit.Length > 5 && short.TryParse(packetsplit[5], out slot) && byte.TryParse(packetsplit[4], out type))
            {
                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot, (InventoryType)type);
                inv?.Item.Use(Session, ref inv, packetsplit[1].ElementAt(0) == '#', packetsplit);
            }
        }

        [Packet("wear")]
        public void Wear(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.Speed == 0)
            {
                return;
            }
            if (packetsplit.Length > 3 && Session.HasCurrentMapInstance && Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null)
            {
                InventoryType type;
                short slot;
                if (Enum.TryParse(packetsplit[3], out type) && short.TryParse(packetsplit[2], out slot))
                {
                    ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot, type);
                    if (inv?.Item != null)
                    {
                        inv.Item.Use(Session, ref inv);
                        Session.SendPacket(Session.Character.GenerateEff(123));
                    }
                }
            }
        }

        private void ChangeSP()
        {
            SpecialistInstance sp = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            WearableInstance fairy = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (sp != null)
            {
                if (Session.Character.GetReputIco() < sp.Item.ReputationMinimum)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"), 0));
                    return;
                }
                if (fairy != null && sp.Item.Element != 0 && fairy.Item.Element != sp.Item.Element && fairy.Item.Element != sp.Item.SecondaryElement)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                    return;
                }
                Session.Character.Buff.Clear();
                Session.Character.LastTransform = DateTime.Now;
                Session.Character.UseSp = true;
                Session.Character.Morph = sp.Item.Morph;
                Session.Character.MorphUpgrade = sp.Upgrade;
                Session.Character.MorphUpgrade2 = sp.Design;
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                Session.SendPacket(Session.Character.GenerateLev());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(196), Session.Character.PositionX, Session.Character.PositionY);
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGuri(6, 1), Session.Character.PositionX, Session.Character.PositionY);
                Session.SendPacket(Session.Character.GenerateSpPoint());
                Session.Character.LoadSpeed();
                Session.SendPacket(Session.Character.GenerateCond());
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateStatChar());
                Session.Character.SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
                foreach (Skill ski in ServerManager.GetAllSkill())
                {
                    if (ski.Class == Session.Character.Morph + 31 && sp.SpLevel >= ski.LevelMinimum)
                    {
                        Session.Character.SkillsSp[ski.SkillVNum] = new CharacterSkill() { SkillVNum = ski.SkillVNum, CharacterId = Session.Character.CharacterId };
                    }
                }
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
            }
        }

        private void CloseExchange(ClientSession session, ClientSession targetSession)
        {
            if (targetSession?.Character.ExchangeInfo != null)
            {
                targetSession.SendPacket("exc_close 0");
                targetSession.Character.ExchangeInfo = null;
            }

            if (session?.Character.ExchangeInfo != null)
            {
                session.SendPacket("exc_close 0");
                session.Character.ExchangeInfo = null;
            }
        }

        private void Exchange(ClientSession sourceSession, ClientSession targetSession)
        {
            if (sourceSession?.Character.ExchangeInfo == null)
            {
                return;
            }

            // remove all items from source session
            foreach (ItemInstance item in sourceSession.Character.ExchangeInfo.ExchangeList)
            {
                ItemInstance invtemp = sourceSession.Character.Inventory.GetItemInstanceById(item.Id);
                if (invtemp != null)
                {
                    sourceSession.Character.Inventory.RemoveItemAmountFromInventory(item.Amount, invtemp.Id);
                }
                else
                {
                    return;
                }
            }

            // add all items to target session
            foreach (ItemInstance item in sourceSession.Character.ExchangeInfo.ExchangeList)
            {
                ItemInstance item2 = item.DeepCopy();
                item2.Id = Guid.NewGuid();
                ItemInstance inv = targetSession.Character.Inventory.AddToInventory(item2);
                if (inv == null || inv.Slot == -1)
                {
                    continue;
                }
                targetSession.SendPacket(targetSession.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
            }

            // handle gold
            sourceSession.Character.Gold -= sourceSession.Character.ExchangeInfo.Gold;
            sourceSession.SendPacket(sourceSession.Character.GenerateGold());
            targetSession.Character.Gold += sourceSession.Character.ExchangeInfo.Gold;
            targetSession.SendPacket(targetSession.Character.GenerateGold());

            // all items and gold from sourceSession have been transferred, clean exchange info
            sourceSession.Character.ExchangeInfo = null;
        }

        private void RemoveSP(short vnum)
        {
            if (Session != null && Session.HasSession)
            {
                if (Session.Character.IsVehicled)
                {
                    return;
                }
                Session.Character.Buff.Clear();

                Logger.Debug(vnum.ToString(), Session.SessionId);
                Session.Character.UseSp = false;
                Session.Character.LoadSpeed();
                Session.SendPacket(Session.Character.GenerateCond());
                Session.SendPacket(Session.Character.GenerateLev());
                Session.Character.SpCooldown = 30;
                if (Session.Character?.SkillsSp != null)
                {
                    foreach (CharacterSkill ski in Session.Character.SkillsSp.GetAllItems().Where(s => !s.CanBeUsed()))
                    {
                        short time = ski.Skill.Cooldown;
                        double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + time * 100;
                        temp /= 1000;
                        Session.Character.SpCooldown = temp > Session.Character.SpCooldown ? (int)temp : Session.Character.SpCooldown;
                    }
                }
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), Session.Character.SpCooldown), 11));
                Session.SendPacket($"sd {Session.Character.SpCooldown}");
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGuri(6, 1), Session.Character.PositionX, Session.Character.PositionY);

                // ms_c
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateStatChar());
                Observable.Timer(TimeSpan.FromMilliseconds(Session.Character.SpCooldown * 1000))
                           .Subscribe(
                           o =>
                           {
                               Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                               Session.SendPacket("sd 0");
                           });
            }
        }

        #endregion
    }
}