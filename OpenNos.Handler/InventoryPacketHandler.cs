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
using OpenNos.Core.Handling;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Packets.ClientPackets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class InventoryPacketHandler : IPacketHandler
    {
        #region Instantiation

        public InventoryPacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// b_i packet
        /// </summary>
        /// <param name="bIPacket"></param>
        public void AskToDelete(BIPacket bIPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), bIPacket.ToString());
            switch (bIPacket.Option)
            {
                case null:
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#b_i^{(byte)bIPacket.InventoryType}^{bIPacket.Slot}^1 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("ASK_TO_DELETE")}"));
                    break;

                case 1:
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#b_i^{(byte)bIPacket.InventoryType}^{bIPacket.Slot}^2 #b_i^{(byte)bIPacket.InventoryType}^{bIPacket.Slot}^5 {Language.Instance.GetMessageFromKey("SURE_TO_DELETE")}"));
                    break;

                case 2:
                    if (Session.Character.InExchangeOrTrade || bIPacket.InventoryType == InventoryType.Bazaar)
                    {
                        return;
                    }
                    Session.Character.DeleteItem(bIPacket.InventoryType, bIPacket.Slot);
                    break;
            }
        }

        /// <summary>
        /// deposit packet
        /// </summary>
        /// <param name="depositPacket"></param>
        public void Deposit(DepositPacket depositPacket)
        {
            ItemInstance item = Session.Character.Inventory.LoadBySlotAndType(depositPacket.Slot, depositPacket.Inventory);
            ItemInstance itemdest = Session.Character.Inventory.LoadBySlotAndType(depositPacket.NewSlot, depositPacket.PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);

            // check if the destination slot is out of range
            if (depositPacket.NewSlot >= (depositPacket.PartnerBackpack ? (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBackPack) ? 50 : 0) : Session.Character.WareHouseSize))
            {
                return;
            }

            // check if the character is allowed to move the item
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            // actually move the item from source to destination
            Session.Character.Inventory.DepositItem(depositPacket.Inventory, depositPacket.Slot, depositPacket.Amount, depositPacket.NewSlot, ref item, ref itemdest, depositPacket.PartnerBackpack);
        }

        /// <summary>
        /// eqinfo packet
        /// </summary>
        /// <param name="equipmentInfoPacket"></param>
        public void EquipmentInfo(EquipmentInfoPacket equipmentInfoPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), equipmentInfoPacket.ToString());
            bool isNPCShopItem = false;
            WearableInstance inventory = null;
            switch (equipmentInfoPacket.Type)
            {
                case 0:
                    inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(equipmentInfoPacket.Slot, InventoryType.Wear) ??
                                Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(equipmentInfoPacket.Slot, InventoryType.Wear);
                    break;

                case 1:
                    inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(equipmentInfoPacket.Slot, InventoryType.Equipment) ??
                                Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(equipmentInfoPacket.Slot, InventoryType.Equipment) ??
                                Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(equipmentInfoPacket.Slot, InventoryType.Equipment);
                    break;

                case 2:
                    isNPCShopItem = true;
                    if (ServerManager.Instance.GetItem(equipmentInfoPacket.Slot) != null)
                    {
                        inventory = new WearableInstance(equipmentInfoPacket.Slot, 1);
                        break;
                    }
                    return;

                case 5:
                    if (Session.Character.ExchangeInfo != null)
                    {
                        ExchangeInfo exch = ServerManager.Instance.GetProperty<ExchangeInfo>(Session.Character.ExchangeInfo.TargetCharacterId, nameof(Character.ExchangeInfo));
                        if (exch?.ExchangeList?.ElementAtOrDefault(equipmentInfoPacket.Slot) != null)
                        {
                            Guid id = exch.ExchangeList.ElementAt(equipmentInfoPacket.Slot).Id;
                            Inventory inv = ServerManager.Instance.GetProperty<Inventory>(Session.Character.ExchangeInfo.TargetCharacterId, nameof(Character.Inventory));
                            inventory = inv.LoadByItemInstance<WearableInstance>(id) ??
                                        inv.LoadByItemInstance<SpecialistInstance>(id) ??
                                        inv.LoadByItemInstance<BoxInstance>(id);
                        }
                    }
                    break;

                case 6:
                    if (equipmentInfoPacket.ShopOwnerId != null)
                    {
                        KeyValuePair<long, MapShop> shop = Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(equipmentInfoPacket.ShopOwnerId));
                        PersonalShopItem item = shop.Value?.Items.FirstOrDefault(i => i.ShopSlot.Equals(equipmentInfoPacket.Slot));
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
                    inventory = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(equipmentInfoPacket.Slot, InventoryType.Specialist);
                    break;

                case 10:
                    inventory = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(equipmentInfoPacket.Slot, InventoryType.Specialist);
                    break;

                case 11:
                    inventory = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(equipmentInfoPacket.Slot, InventoryType.Costume);
                    break;
            }
            if (inventory?.Item != null)
            {
                if (inventory.IsEmpty || isNPCShopItem)
                {
                    Session.SendPacket(inventory.GenerateEInfo());
                    return;
                }
                Session.SendPacket(inventory.Item.EquipmentSlot != EquipmentType.Sp ? inventory.GenerateEInfo() : inventory.Item.SpType == 0 && inventory.Item.ItemSubType == 4 ? (inventory as SpecialistInstance)?.GeneratePslInfo() : (inventory as SpecialistInstance)?.GenerateSlInfo());
            }
        }

        // TODO: TRANSLATE IT TO PACKETDEFINITION!
        [Packet("exc_list")]
        public void ExchangeList(string packet)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), packet);
            string[] packetsplit = packet.Split(' ');
            if (!long.TryParse(packetsplit[2], out long gold))
            {
                return;
            }
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

            for (int j = 6, i = 0; j <= packetsplit.Length && i < 10; j += 3, i++)
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

        /// <summary>
        /// req_exc packet
        /// </summary>
        /// <param name="exchangeRequestPacket"></param>
        public void ExchangeRequest(ExchangeRequestPacket exchangeRequestPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), exchangeRequestPacket.ToString());
            if (exchangeRequestPacket.CharacterId != 0 && Session.Character.MapInstanceId != ServerManager.Instance.GetProperty<Guid>(exchangeRequestPacket.CharacterId, nameof(Character.MapInstanceId)))
            {
                ServerManager.Instance.SetProperty(exchangeRequestPacket.CharacterId, nameof(Character.ExchangeInfo), null);
                Session.Character.ExchangeInfo = null;
            }
            else
            {
                switch (exchangeRequestPacket.RequestType)
                {
                    case RequestExchangeType.Requested:
                        if (!Session.HasCurrentMapInstance)
                        {
                            return;
                        }
                        ClientSession targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(exchangeRequestPacket.CharacterId);
                        if (targetSession == null)
                        {
                            return;
                        }

                        if (targetSession.Character.Group != null && targetSession.Character.Group?.GroupType != GroupType.Group)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                            return;
                        }

                        if (Session.Character.Group != null && Session.Character.Group?.GroupType != GroupType.Group)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"), 0));
                            return;
                        }

                        if (Session.Character.IsBlockedByCharacter(exchangeRequestPacket.CharacterId))
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                            return;
                        }

                        if (Session.Character.Speed == 0 || targetSession.Character.Speed == 0)
                        {
                            Session.Character.ExchangeBlocked = true;
                        }
                        if (targetSession.Character.LastSkillUse.AddSeconds(20) > DateTime.Now || targetSession.Character.LastDefence.AddSeconds(20) > DateTime.Now)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("PLAYER_IN_BATTLE"), targetSession.Character.Name)));
                            return;
                        }

                        if (Session.Character.LastSkillUse.AddSeconds(20) > DateTime.Now || Session.Character.LastDefence.AddSeconds(20) > DateTime.Now)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("IN_BATTLE")));
                            return;
                        }

                        if (Session.Character.HasShopOpened || targetSession.Character.HasShopOpened)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HAS_SHOP_OPENED"), 10));
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
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0));
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(string.Format(Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE"), targetSession.Character.Name), 0));
                                Session.Character.TradeRequests.Add(targetSession.Character.CharacterId);
                                targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#req_exc^2^{Session.Character.CharacterId} #req_exc^5^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INCOMING_EXCHANGE"), Session.Character.Name)}"));
                            }
                        }
                        break;

                    case RequestExchangeType.Confirmed: // click Trade button in exchange window
                        if (Session.HasCurrentMapInstance && Session.HasSelectedCharacter
                            && Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.TargetCharacterId != Session.Character.CharacterId)
                        {
                            if (!Session.HasCurrentMapInstance)
                            {
                                return;
                            }
                            targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);

                            if (targetSession == null)
                            {
                                return;
                            }
                            if (Session.Character.Group != null && Session.Character.Group?.GroupType != GroupType.Group)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                                return;
                            }

                            if (targetSession.Character.Group != null && targetSession.Character.Group?.GroupType != GroupType.Group)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"), 0));
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
                                    long maxGold = ServerManager.Instance.MaxGold;

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
                                            if (!Session.Character.Inventory.EnoughPlace(targetExchange.ExchangeList, Session.Character.HaveBackpack() ? 1 : 0))
                                            {
                                                @continue = false;
                                            }
                                            if (!inventory.EnoughPlace(Session.Character.ExchangeInfo.ExchangeList, backpack))
                                            {
                                                @continue = false;
                                            }
                                            if (Session.Character.ExchangeInfo.Gold + gold > maxGold)
                                            {
                                                goldmax = true;
                                            }
                                            if (Session.Character.ExchangeInfo.Gold > Session.Character.Gold)
                                            {
                                                return;
                                            }
                                            if (targetExchange.Gold + Session.Character.Gold > maxGold)
                                            {
                                                goldmax = true;
                                            }
                                            if (!@continue || goldmax)
                                            {
                                                string message = !@continue ? UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0)
                                                    : UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                                                Session.SendPacket(message);
                                                targetSession.SendPacket(message);
                                                CloseExchange(Session, targetSession);
                                            }
                                            else
                                            {
                                                if (Session.Character.ExchangeInfo.ExchangeList.Any(ei => !(ei.Item.IsTradable || ei.IsBound)))
                                                {
                                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_TRADABLE"), 0));
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
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("IN_WAITING_FOR"), targetSession.Character.Name)));
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    case RequestExchangeType.Cancelled: // cancel trade thru exchange window
                        if (Session.HasCurrentMapInstance && Session.Character.ExchangeInfo != null)
                        {
                            targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);
                            CloseExchange(Session, targetSession);
                        }
                        break;

                    case RequestExchangeType.List:
                        bool otherInExchangeOrTrade = ServerManager.Instance.GetProperty<bool>(exchangeRequestPacket.CharacterId, nameof(Character.InExchangeOrTrade));
                        if (!Session.Character.InExchangeOrTrade || !otherInExchangeOrTrade)
                        {
                            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(exchangeRequestPacket.CharacterId);
                            if (exchangeRequestPacket.CharacterId == Session.Character.CharacterId || Session.Character.Speed == 0 || otherSession == null || otherSession.Character.TradeRequests.All(s => s != Session.Character.CharacterId))
                            {
                                return;
                            }
                            if (Session.Character.Group != null && Session.Character.Group?.GroupType != GroupType.Group)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                                return;
                            }

                            if (otherSession.Character.Group != null && otherSession.Character.Group?.GroupType != GroupType.Group)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"), 0));
                                return;
                            }

                            Session.SendPacket($"exc_list 1 {exchangeRequestPacket.CharacterId} -1");
                            ExchangeInfo exc = new ExchangeInfo
                            {
                                TargetCharacterId = exchangeRequestPacket.CharacterId,
                                Confirm = false
                            };
                            Session.Character.ExchangeInfo = exc;
                            ServerManager.Instance.SetProperty(exchangeRequestPacket.CharacterId, nameof(Character.ExchangeInfo), new ExchangeInfo { TargetCharacterId = Session.Character.CharacterId, Confirm = false });
                            Session.CurrentMapInstance?.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} -1", ReceiverType.OnlySomeone, string.Empty, exchangeRequestPacket.CharacterId);
                        }
                        else
                        {
                            Session.CurrentMapInstance?.Broadcast(Session, UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0), ReceiverType.OnlySomeone, string.Empty, exchangeRequestPacket.CharacterId);
                        }
                        break;

                    case RequestExchangeType.Declined:
                        ServerManager.Instance.GetProperty<string>(exchangeRequestPacket.CharacterId, nameof(Character.Name));
                        ServerManager.Instance.SetProperty(exchangeRequestPacket.CharacterId, nameof(Character.ExchangeInfo), null);
                        Session.Character.ExchangeInfo = null;
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("YOU_REFUSED"), 10));
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EXCHANGE_REFUSED"), Session.Character.Name), 10), ReceiverType.OnlySomeone, string.Empty, exchangeRequestPacket.CharacterId);
                        break;

                    default:
                        Logger.Log.Warn($"Exchange-Request-Type not implemented. RequestType: {exchangeRequestPacket.RequestType})");
                        break;
                }
            }
        }

        /// <summary>
        /// get packet
        /// </summary>
        /// <param name="getPacket"></param>
        public void GetItem(GetPacket getPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), getPacket.ToString());
            if (Session.Character.LastSkillUse.AddSeconds(1) > DateTime.Now || Session.Character.IsVehicled || !Session.HasCurrentMapInstance)
            {
                return;
            }

            if (getPacket.TransportId < 100000)
            {
                MapButton button = Session.CurrentMapInstance.Buttons.FirstOrDefault(s => s.MapButtonId == getPacket.TransportId);
                if (button != null)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(2000, 1, $"#git^{button.MapButtonId}"));
                }
            }
            else
            {
                if (!Session.CurrentMapInstance.DroppedList.ContainsKey(getPacket.TransportId))
                {
                    return;
                }

                MapItem mapItem = Session.CurrentMapInstance.DroppedList[getPacket.TransportId];

                if (mapItem != null)
                {
                    bool canpick = false;
                    switch (getPacket.PickerType)
                    {
                        case 1:
                            canpick = Session.Character.IsInRange(mapItem.PositionX, mapItem.PositionY, 8);
                            break;

                        case 2:
                            Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == getPacket.PickerId && s.CanPickUp);
                            if (mate != null)
                            {
                                canpick = mate.IsInRange(mapItem.PositionX, mapItem.PositionY, 8);
                            }
                            break;
                    }
                    if (canpick && Session.HasCurrentMapInstance)
                    {
                        if (mapItem is MonsterMapItem item)
                        {
                            MonsterMapItem monsterMapItem = item;
                            if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance && monsterMapItem.OwnerId.HasValue && monsterMapItem.OwnerId.Value != -1)
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
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"), mapItem.GetItemInstance().Item.EffectValue), 0));
                                    Session.SendPacket(Session.Character.GenerateSpPoint());
                                }
                                Session.CurrentMapInstance.DroppedList.Remove(getPacket.TransportId);
                                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGet(getPacket.TransportId));
                            }
                            else
                            {
                                lock (Session.Character.Inventory)
                                {
                                    byte amount = mapItem.Amount;
                                    ItemInstance inv = Session.Character.Inventory.AddToInventory(mapItemInstance).FirstOrDefault();
                                    if (inv != null)
                                    {
                                        Session.CurrentMapInstance.DroppedList.Remove(getPacket.TransportId);
                                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGet(getPacket.TransportId));
                                        if (getPacket.PickerType == 2)
                                        {
                                            Session.SendPacket(Session.Character.GenerateIcon(1, 1, inv.ItemVNum));
                                        }
                                        Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {inv.Item.Name} x {amount}", 12));
                                        if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.LodInstance)
                                        {
                                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay($"{string.Format(Language.Instance.GetMessageFromKey("ITEM_ACQUIRED_LOD"), Session.Character.Name)}: {inv.Item.Name} x {mapItem.Amount}", 10));
                                        }
                                    }
                                    else
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // handle gold drop
                            long maxGold = ServerManager.Instance.MaxGold;
                            if (mapItem is MonsterMapItem droppedGold && Session.Character.Gold + droppedGold.GoldAmount <= maxGold)
                            {
                                if (getPacket.PickerType == 2)
                                {
                                    Session.SendPacket(Session.Character.GenerateIcon(1, 1, 1046));
                                }
                                Session.Character.Gold += droppedGold.GoldAmount;
                                Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {mapItem.GetItemInstance().Item.Name} x {droppedGold.GoldAmount}", 12));
                            }
                            else
                            {
                                Session.Character.Gold = maxGold;
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                            }
                            Session.SendPacket(Session.Character.GenerateGold());
                            Session.CurrentMapInstance.DroppedList.Remove(getPacket.TransportId);
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGet(getPacket.TransportId));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// mve packet
        /// </summary>
        /// <param name="mvePacket"></param>
        public void MoveEquipment(MvePacket mvePacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), mvePacket.ToString());
            lock (Session.Character.Inventory)
            {
                if(mvePacket.Slot.Equals(mvePacket.DestinationSlot) && mvePacket.InventoryType.Equals(mvePacket.DestinationInventoryType))
                {
                    return;
                }
                if (mvePacket.DestinationSlot > 48 + (Session.Character.HaveBackpack() ? 1 : 0) * 12)
                {
                    return;
                }
                if (Session.Character.InExchangeOrTrade)
                {
                    return;
                }
                ItemInstance sourceItem = Session.Character.Inventory.LoadBySlotAndType(mvePacket.Slot, mvePacket.InventoryType);
                if (sourceItem != null && sourceItem.Item.ItemType == ItemType.Specialist || sourceItem != null && sourceItem.Item.ItemType == ItemType.Fashion)
                {
                    ItemInstance inv = Session.Character.Inventory.MoveInInventory(mvePacket.Slot, mvePacket.InventoryType, mvePacket.DestinationInventoryType, mvePacket.DestinationSlot, false);
                    if (inv != null)
                    {
                        Session.SendPacket(inv.GenerateInventoryAdd());
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(mvePacket.InventoryType, mvePacket.Slot));
                    }
                }
            }
        }

        /// <summary>
        /// mvi packet
        /// </summary>
        /// <param name="mviPacket"></param>
        public void MoveItem(MviPacket mviPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), mviPacket.ToString());
            lock (Session.Character.Inventory)
            {
                if(mviPacket.Amount == 0)
                {
                    return;
                }

                if (mviPacket.Slot == mviPacket.DestinationSlot)
                {
                    return;
                }

                // check if the destination slot is out of range
                if (mviPacket.DestinationSlot > 48 + (Session.Character.HaveBackpack() ? 1 : 0) * 12)
                {
                    return;
                }

                // check if the character is allowed to move the item
                if (Session.Character.InExchangeOrTrade)
                {
                    return;
                }

                // actually move the item from source to destination
                Session.Character.Inventory.MoveItem(mviPacket.InventoryType, mviPacket.InventoryType, mviPacket.Slot, mviPacket.Amount, mviPacket.DestinationSlot, out ItemInstance previousInventory, out ItemInstance newInventory);
                if (newInventory == null)
                {
                    return;
                }
                Session.SendPacket(newInventory.GenerateInventoryAdd());

                Session.SendPacket(previousInventory != null
                    ? previousInventory.GenerateInventoryAdd()
                    : UserInterfaceHelper.Instance.GenerateInventoryRemove(mviPacket.InventoryType, mviPacket.Slot));
            }
        }

        /// <summary>
        /// put packet
        /// </summary>
        /// <param name="putPacket"></param>
        public void PutItem(PutPacket putPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), putPacket.ToString());
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
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE_HERE"), 0));
                                return;
                            }
                            Session.SendPacket(invitem.GenerateInventoryAdd());

                            if (invitem.Amount == 0)
                            {
                                Session.Character.DeleteItem(invitem.Type, invitem.Slot);
                            }
                            Session.CurrentMapInstance?.Broadcast($"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.Amount} 0 -1");
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_MAP_FULL"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DROP_AMOUNT"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
                }
            }
        }

        /// <summary>
        /// remove packet
        /// </summary>
        /// <param name="removePacket"></param>
        public void Remove(RemovePacket removePacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), removePacket.ToString());
            InventoryType equipment;
            Mate mate = null;
            switch (removePacket.Type)
            {
                case 1:
                    equipment = InventoryType.FirstPartnerInventory;
                    mate = Session.Character.Mates.FirstOrDefault(s => s.PetId == removePacket.Type - 1);
                    break;

                case 2:
                    equipment = InventoryType.SecondPartnerInventory;
                    mate = Session.Character.Mates.FirstOrDefault(s => s.PetId == removePacket.Type - 1);
                    break;

                case 3:
                    equipment = InventoryType.ThirdPartnerInventory;
                    mate = Session.Character.Mates.FirstOrDefault(s => s.PetId == removePacket.Type - 1);
                    break;

                default:
                    equipment = InventoryType.Wear;
                    break;
            }

            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null && (Session.Character.ExchangeInfo == null || !(Session.Character.ExchangeInfo?.ExchangeList).Any()))
            {
                ItemInstance inventory = removePacket.InventorySlot != (byte)EquipmentType.Sp ? Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(removePacket.InventorySlot, equipment) : Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(removePacket.InventorySlot, equipment);
                if (inventory != null)
                {
                    double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                    if (removePacket.Type == 0)
                    {
                        if (removePacket.InventorySlot == (byte)EquipmentType.Sp && Session.Character.UseSp)
                        {
                            if (Session.Character.IsVehicled)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                                return;
                            }
                            if (Session.Character.LastSkillUse.AddSeconds(2) > DateTime.Now)
                            {
                                return;
                            }
                            Session.Character.LastSp = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                            RemoveSP(inventory.ItemVNum);
                        }
                        else if (removePacket.InventorySlot == (byte)EquipmentType.Sp && !Session.Character.UseSp && timeSpanSinceLastSpUsage <= Session.Character.SpCooldown)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                            return;
                        }
                        Session.Character.EquipmentBCards.RemoveAll(o => o.ItemVNum == inventory.ItemVNum);
                    }

                    ItemInstance inv = Session.Character.Inventory.MoveInInventory(removePacket.InventorySlot, equipment, InventoryType.Equipment);

                    if (inv == null)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        return;
                    }

                    if (inv.Slot != -1)
                    {
                        Session.SendPacket(inventory.GenerateInventoryAdd());
                    }
                    if (removePacket.Type == 0)
                    {
                        Session.SendPacket(Session.Character.GenerateStatChar());
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEq());
                        Session.SendPacket(Session.Character.GenerateEquipment());
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GeneratePairy());
                    }
                    else if (mate != null)
                    {
                        Session.SendPacket(mate.GenerateScPacket());
                    }

                }
            }
        }

        /// <summary>
        /// repos packet
        /// </summary>
        /// <param name="reposPacket"></param>
        public void Repos(ReposPacket reposPacket)
        {
            if(reposPacket.OldSlot.Equals(reposPacket.NewSlot))
            {
                return;
            }

            if(reposPacket.Amount == 0)
            {
                return;
            }

            // check if the destination slot is out of range
            if (reposPacket.NewSlot >= (reposPacket.PartnerBackpack ? (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBackPack) ? 50 : 0) : Session.Character.WareHouseSize))
            {
                return;
            }

            // check if the character is allowed to move the item
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            // actually move the item from source to destination
            Session.Character.Inventory.MoveItem(reposPacket.PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse, reposPacket.PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse, reposPacket.OldSlot, reposPacket.Amount, reposPacket.NewSlot, out ItemInstance previousInventory, out ItemInstance newInventory);
            if (newInventory == null)
            {
                return;
            }

            Session.SendPacket(reposPacket.PartnerBackpack ? newInventory.GeneratePStash() : newInventory.GenerateStash());
            Session.SendPacket(previousInventory != null ? (reposPacket.PartnerBackpack ? previousInventory.GeneratePStash() : previousInventory.GenerateStash()) : (reposPacket.PartnerBackpack ? UserInterfaceHelper.Instance.GeneratePStashRemove(reposPacket.OldSlot) : UserInterfaceHelper.Instance.GenerateStashRemove(reposPacket.OldSlot)));
        }

        [Packet("sortopen")]
        public void SortOpen(string packet)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), packet);
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
                                Session.Character.Inventory.MoveItem(type, type, (short)(x + 1), 1, x, out ItemInstance inv, out ItemInstance invdest);
                                if (invdest is WearableInstance wearableInstance)
                                {
                                    Session.SendPacket(invdest.GenerateInventoryAdd());
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

        /// <summary>
        /// s_carrier packet
        /// </summary>
        /// <param name="specialistHolderPacket"></param>
        public void SpecialistHolder(SpecialistHolderPacket specialistHolderPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), specialistHolderPacket.ToString());
            SpecialistInstance specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(specialistHolderPacket.Slot, InventoryType.Equipment);
            BoxInstance holder = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(specialistHolderPacket.HolderSlot, InventoryType.Equipment);
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

        /// <summary>
        /// sl packet
        /// </summary>
        /// <param name="spTransformPacket"></param>
        public void SpTransform(SpTransformPacket spTransformPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), spTransformPacket.ToString());

            SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);

            if (spTransformPacket.Type == 10)
            {
                short specialistDamage = spTransformPacket.SpecialistDamage, specialistDefense = spTransformPacket.SpecialistDefense, specialistElement = spTransformPacket.SpecialistElement, specialistHealpoints = spTransformPacket.SpecialistHP;
                int transportId = spTransformPacket.TransportId;
                if (!Session.Character.UseSp || specialistInstance == null || transportId != specialistInstance.TransportId)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SPUSE_NEEDED"), 0));
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
                Session.SendPacket(specialistInstance.GenerateSlInfo());
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_SET"), 0));
            }
            else if (!Session.Character.IsSitting)
            {
                if (Session.Character.Skills.GetAllItems().Any(s => !s.CanBeUsed()))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILLS_IN_LOADING"), 0));
                    return;
                }
                if (specialistInstance == null)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    return;
                }
                if (Session.Character.IsVehicled)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
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
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SP_NOPOINTS"), 0));
                    }
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                    if (timeSpanSinceLastSpUsage >= Session.Character.SpCooldown)
                    {
                        // TODO: add check on packetheader instead of this type check, way to abuse
                        if (spTransformPacket.Type == 1)
                        {
                            DateTime delay = DateTime.Now.AddSeconds(-6);
                            if (Session.Character.LastDelay > delay && Session.Character.LastDelay < delay.AddSeconds(2))
                            {
                                ChangeSP();
                            }
                        }
                        else
                        {
                            Session.Character.LastDelay = DateTime.Now;
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 3, "#sl^1"));
                            Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);
                        }
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                    }
                }
            }
        }

        /// <summary>
        /// up_gr packet
        /// </summary>
        /// <param name="upgradePacket"></param>
        public void Upgrade(UpgradePacket upgradePacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), upgradePacket.ToString());
            if (Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.Speed == 0)
            {
                return;
            }
            if (Session.Character.LastDelay.AddSeconds(5) > DateTime.Now)
            {
                return;
            }
            InventoryType inventoryType = upgradePacket.InventoryType;
            byte uptype = upgradePacket.UpgradeType, slot = upgradePacket.Slot;
            Session.Character.LastDelay = DateTime.Now;
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
                    if (upgradePacket.InventoryType2 != null && upgradePacket.Slot2 != null)
                    {
                        WearableInstance inventory2 = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)upgradePacket.Slot2, (InventoryType)upgradePacket.InventoryType2);

                        if (inventory != null && inventory2 != null && !Equals(inventory, inventory2))
                        {
                            inventory.Sum(Session, inventory2);
                        }
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
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
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
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
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
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
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
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
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

        /// <summary>
        /// u_i packet
        /// </summary>
        /// <param name="useItemPacket"></param>
        public void UseItem(UseItemPacket useItemPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), useItemPacket.ToString());
            if ((byte)useItemPacket.Type >= 9)
            {
                return;
            }
            ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(useItemPacket.Slot, useItemPacket.Type);
            string[] packetsplit = useItemPacket.OriginalContent.Split(' ', '^');
            inv?.Item.Use(Session, ref inv, packetsplit[1].ElementAt(0) == '#' ? (byte)255 : (byte)0, packetsplit);
        }

        /// <summary>
        /// wear packet
        /// </summary>
        /// <param name="wearPacket"></param>
        public void Wear(WearPacket wearPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), wearPacket.ToString());
            if (Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.Speed == 0)
            {
                return;
            }
            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null)
            {
                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(wearPacket.InventorySlot, InventoryType.Equipment);
                if (inv?.Item != null)
                {
                    inv.Item.Use(Session, ref inv, wearPacket.Type);
                    Session.SendPacket(Session.Character.GenerateEff(123));
                }
            }
        }

        /// <summary>
        /// withdraw packet
        /// </summary>
        /// <param name="withdrawPacket"></param>
        public void Withdraw(WithdrawPacket withdrawPacket)
        {
            ItemInstance previousInventory = Session.Character.Inventory.LoadBySlotAndType(withdrawPacket.Slot, withdrawPacket.PetBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);
            if (withdrawPacket.Amount <= 0 || previousInventory == null || withdrawPacket.Amount > previousInventory.Amount || !Session.Character.Inventory.CanAddItem(previousInventory.ItemVNum))
            {
                return;
            }
            ItemInstance item2 = previousInventory.DeepCopy();
            item2.Id = Guid.NewGuid();
            item2.Amount = withdrawPacket.Amount;
            Session.Character.Inventory.RemoveItemAmountFromInventory(withdrawPacket.Amount, previousInventory.Id);
            Session.Character.Inventory.AddToInventory(item2, item2.Item.Type);
            previousInventory = Session.Character.Inventory.LoadBySlotAndType(withdrawPacket.Slot, withdrawPacket.PetBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);
            Session.SendPacket(withdrawPacket.PetBackpack ? UserInterfaceHelper.Instance.GeneratePStashRemove(withdrawPacket.Slot) : UserInterfaceHelper.Instance.GenerateStashRemove(withdrawPacket.Slot));
        }

        /// <summary>
        /// changesp private method
        /// </summary>
        private void ChangeSP()
        {
            SpecialistInstance sp = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            WearableInstance fairy = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (sp != null)
            {
                if (Session.Character.GetReputIco() < sp.Item.ReputationMinimum)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"), 0));
                    return;
                }
                if (fairy != null && sp.Item.Element != 0 && fairy.Item.Element != sp.Item.Element && fairy.Item.Element != sp.Item.SecondaryElement)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                    return;
                }
                List<BuffType> bufftodisable = new List<BuffType>();
                bufftodisable.Add(BuffType.Bad);
                bufftodisable.Add(BuffType.Good);
                bufftodisable.Add(BuffType.Neutral);
                Session.Character.DisableBuffs(bufftodisable);
                Session.Character.LastTransform = DateTime.Now;
                Session.Character.UseSp = true;
                Session.Character.Morph = sp.Item.Morph;
                Session.Character.MorphUpgrade = sp.Upgrade;
                Session.Character.MorphUpgrade2 = sp.Design;
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                Session.SendPacket(Session.Character.GenerateLev());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(196), Session.Character.PositionX, Session.Character.PositionY);
                Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(6, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);
                Session.SendPacket(Session.Character.GenerateSpPoint());
                Session.Character.LoadSpeed();
                Session.SendPacket(Session.Character.GenerateCond());
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateStatChar());
                Session.Character.SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
                Parallel.ForEach(ServerManager.Instance.GetAllSkill(), skill =>
                {
                    if (skill.Class == Session.Character.Morph + 31 && sp.SpLevel >= skill.LevelMinimum)
                    {
                        Session.Character.SkillsSp[skill.SkillVNum] = new CharacterSkill { SkillVNum = skill.SkillVNum, CharacterId = Session.Character.CharacterId };
                    }
                });
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
            }
        }

        /// <summary>
        /// exchange closure method
        /// </summary>
        /// <param name="session"></param>
        /// <param name="targetSession"></param>
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

        /// <summary>
        /// exchange initialization method
        /// </summary>
        /// <param name="sourceSession"></param>
        /// <param name="targetSession"></param>
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
                if (invtemp != null && invtemp.Amount >= item.Amount)
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
                List<ItemInstance> inv = targetSession.Character.Inventory.AddToInventory(item2);
                if (!inv.Any())
                {
                    // do what?
                }
            }

            // handle gold
            sourceSession.Character.Gold -= sourceSession.Character.ExchangeInfo.Gold;
            sourceSession.SendPacket(sourceSession.Character.GenerateGold());
            targetSession.Character.Gold += sourceSession.Character.ExchangeInfo.Gold;
            targetSession.SendPacket(targetSession.Character.GenerateGold());

            // all items and gold from sourceSession have been transferred, clean exchange info
            sourceSession.Character.ExchangeInfo = null;
        }

        /// <summary>
        /// sp removal method
        /// </summary>
        /// <param name="vnum"></param>
        private void RemoveSP(short vnum)
        {
            if (Session != null && Session.HasSession)
            {
                if (Session.Character.IsVehicled)
                {
                    return;
                }
                List<BuffType> bufftodisable = new List<BuffType>();
                bufftodisable.Add(BuffType.Bad);
                bufftodisable.Add(BuffType.Good);
                bufftodisable.Add(BuffType.Neutral);
                Session.Character.DisableBuffs(bufftodisable);
                Logger.Debug(Session.Character.GenerateIdentity(), vnum.ToString());
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
                Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(6, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);

                // ms_c
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateStatChar());
                Observable.Timer(TimeSpan.FromMilliseconds(Session.Character.SpCooldown * 1000)).Subscribe(o =>
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                    Session.SendPacket("sd 0");
                });
            }
        }

        #endregion
    }
}