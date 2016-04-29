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
using System;
using System.Diagnostics;
using System.Threading;

namespace OpenNos.GameObject
{
    public class WearableItem : Item
    {
        #region Methods

        public override void Use(ClientSession Session, ref Inventory inventory)
        {
            switch (Effect)
            {
                default:
                    short slot = inventory.Slot;
                    byte type = inventory.Type;
                    if (inventory == null) return;

                    Item iteminfo = ServerManager.GetItem(inventory.ItemInstance.ItemVNum);
                    if (iteminfo == null) return;

                    if (iteminfo.ItemValidTime > 0 && inventory.ItemInstance.IsUsed == false)
                    {
                        inventory.ItemInstance.ItemDeleteTime = DateTime.Now.AddSeconds(iteminfo.ItemValidTime);
                    }
                    inventory.ItemInstance.IsUsed = true;
                    double timeSpanSinceLastSpUsage = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds -
                                                      Session.Character.LastSp;
                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp && timeSpanSinceLastSpUsage < 30)
                    {
                        Session.Client.SendPacket(
                            Session.Character.GenerateMsg(
                                string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"),
                                    30 - (int)Math.Round(timeSpanSinceLastSpUsage)),
                                0));
                        return;
                    }

                    if ((iteminfo.ItemType != (byte)Domain.ItemType.Weapon
                         && iteminfo.ItemType != (byte)Domain.ItemType.Armor
                         && iteminfo.ItemType != (byte)Domain.ItemType.Fashion
                         && iteminfo.ItemType != (byte)Domain.ItemType.Jewelery
                         && iteminfo.ItemType != (byte)Domain.ItemType.Specialist)
                        || iteminfo.LevelMinimum > (iteminfo.IsHeroic ? Session.Character.HeroLevel : Session.Character.Level) || (iteminfo.Sex != 0 && iteminfo.Sex != Session.Character.Gender + 1)
                        || ((iteminfo.ItemType != (byte)Domain.ItemType.Jewelery && iteminfo.EquipmentSlot != (byte)EquipmentType.Boots && iteminfo.EquipmentSlot != (byte)EquipmentType.Gloves) && ((iteminfo.Class >> Session.Character.Class) & 1) != 1))
                    {
                        Session.Client.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                        return;
                    }

                    if (Session.Character.UseSp
                        && iteminfo.EquipmentSlot == (byte)EquipmentType.Fairy
                        && iteminfo.Element != ServerManager.GetItem(
                            Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>(
                                (byte)EquipmentType.Sp,
                                (byte)InventoryType.Equipment).ItemVNum).Element &&
                                iteminfo.Element != ServerManager.GetItem(
                            Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>(
                                (byte)EquipmentType.Sp,
                                (byte)InventoryType.Equipment).ItemVNum).SecondaryElement)
                    {
                        Session.Client.SendPacket(
                            Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                        return;
                    }

                    if (Session.Character.UseSp && iteminfo.EquipmentSlot == (byte)EquipmentType.Sp)
                    {
                        Session.Client.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SP_BLOCKED"), 10));
                        return;
                    }

                    if (Session.Character.JobLevel < iteminfo.LevelJobMinimum)
                    {
                        Session.Client.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 10));
                        return;
                    }

                    Inventory equip = Session.Character.EquipmentList.LoadInventoryBySlotAndType(iteminfo.EquipmentSlot, (byte)InventoryType.Equipment);
                    if (equip == null)
                    {

                        Session.Character.EquipmentList.AddToInventoryWithSlotAndType( inventory.ItemInstance as ItemInstance, (byte)InventoryType.Equipment, iteminfo.EquipmentSlot);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
                        Session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);

                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                    }
                    else
                    {
                     
                        Session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                        Session.Character.EquipmentList.DeleteFromSlotAndType(equip.Slot, equip.Type);

                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inventory.Type, inventory.Slot, 0, 0, 0));

                        Session.Character.EquipmentList.AddToInventoryWithSlotAndType(inventory.ItemInstance as ItemInstance, (byte)InventoryType.Equipment, iteminfo.EquipmentSlot);
                        Session.Character.InventoryList.AddToInventoryWithSlotAndType(equip.ItemInstance as ItemInstance, type, slot);

                        Session.Client.SendPacket(
                            Session.Character.GenerateInventoryAdd(equip.ItemInstance.ItemVNum,
                                equip.ItemInstance.Amount, type, slot,
                                equip.ItemInstance.Rare, equip.ItemInstance.Design,
                                equip.ItemInstance.Upgrade));

                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                    }

                    break;
            }
        }

        #endregion
    }
}