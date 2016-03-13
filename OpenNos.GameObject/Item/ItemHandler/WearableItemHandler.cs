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
    public class WearableItemHandler
    {
        internal void UseItemHandler(ref Inventory inventory, ClientSession Session, short effect, int effectValue)
        {
            switch (effect)
            {
                default:
                    short slot = inventory.Slot;
                    byte type = inventory.Type;
                    if (inventory == null) return;

                    Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                    if (iteminfo == null) return;
                   
                    if (iteminfo.ItemValidTime > 0 && inventory.InventoryItem.IsUsed == false)
                    {
                        inventory.InventoryItem.ItemDeleteTime = DateTime.Now.AddSeconds(iteminfo.ItemValidTime/10);
                    }
                    inventory.InventoryItem.IsUsed = true;
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

                    if ((iteminfo.ItemType != (byte)ItemType.Weapon
                         && iteminfo.ItemType != (byte)ItemType.Armor
                         && iteminfo.ItemType != (byte)ItemType.Fashion
                         && iteminfo.ItemType != (byte)ItemType.Jewelery
                         && iteminfo.ItemType != (byte)ItemType.Specialist)
                        || iteminfo.LevelMinimum > Session.Character.Level || (iteminfo.Sex != 0 && iteminfo.Sex != Session.Character.Gender + 1)
                        || ((iteminfo.ItemType != (byte)ItemType.Jewelery && iteminfo.EquipmentSlot != (byte)EquipmentType.Boots && iteminfo.EquipmentSlot != (byte)EquipmentType.Gloves) && ((iteminfo.Class >> Session.Character.Class) & 1) != 1))
                    {
                        Session.Client.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                        return;
                    }

                    if (Session.Character.UseSp
                        && iteminfo.EquipmentSlot == (byte)EquipmentType.Fairy
                        && iteminfo.Element != ServerManager.GetItem(
                            Session.Character.EquipmentList.LoadBySlotAndType(
                                (byte)EquipmentType.Sp,
                                (byte)InventoryType.Equipment).InventoryItem.ItemVNum).Element &&
                                iteminfo.Element != ServerManager.GetItem(
                            Session.Character.EquipmentList.LoadBySlotAndType(
                                (byte)EquipmentType.Sp,
                                (byte)InventoryType.Equipment).InventoryItem.ItemVNum).SecondaryElement)
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

                    Inventory equip = Session.Character.EquipmentList.LoadBySlotAndType(iteminfo.EquipmentSlot, (byte)InventoryType.Equipment);
                    if (equip == null)
                    {
                        inventory.Type = (byte)InventoryType.Equipment;
                        inventory.Slot = iteminfo.EquipmentSlot;

                        Session.Character.EquipmentList.InsertOrUpdate(ref inventory);
                        Session.Character.InventoryList.DeleteFromSlotAndType(slot, type);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
                        Session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);

                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                        Thread.Sleep(100);
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                    }
                    else
                    {
                        inventory.Type = (byte)InventoryType.Equipment;
                        inventory.Slot = iteminfo.EquipmentSlot;

                        equip.Slot = slot;
                        equip.Type = type;

                        Session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inventory.Type, inventory.Slot, 0, 0, 0));
                        Session.Character.EquipmentList.DeleteFromSlotAndType(slot, type);

                        Session.Character.InventoryList.InsertOrUpdate(ref equip);
                        Session.Character.EquipmentList.InsertOrUpdate(ref inventory);
                        Session.Client.SendPacket(
                            Session.Character.GenerateInventoryAdd(equip.InventoryItem.ItemVNum,
                                equip.InventoryItem.Amount, type, equip.Slot,
                                equip.InventoryItem.Rare, equip.InventoryItem.Design,
                                equip.InventoryItem.Upgrade));

                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
                        Thread.Sleep(100);
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
                    }
            
            break;
            }

        }
    }
}