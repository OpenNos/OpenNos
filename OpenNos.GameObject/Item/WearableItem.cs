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

namespace OpenNos.GameObject
{
    public class WearableItem : Item
    {
        #region Methods

        public override void Use(ClientSession session, ref Inventory inventory, bool DelayUsed = false)
        {
            switch (Effect)
            {
                default:
                    short slot = inventory.Slot;
                    InventoryType type = inventory.Type;

                    if (inventory == null)
                    {
                        return;
                    }
                    if (ItemValidTime > 0 && ((ItemInstance)inventory.ItemInstance).IsBound)
                    {
                        inventory.ItemInstance.ItemDeleteTime = DateTime.Now.AddSeconds(ItemValidTime);
                    }
                    if (!((ItemInstance)inventory.ItemInstance).IsBound)
                    {
                        if (!DelayUsed && ((EquipmentSlot == (byte)EquipmentType.Fairy && (MaxElementRate == 70 || MaxElementRate == 80)) || (EquipmentSlot == (byte)EquipmentType.CostumeHat || EquipmentSlot == (byte)EquipmentType.CostumeSuit || EquipmentSlot == (byte)EquipmentType.WeaponSkin)))
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)type}^{slot}^1 {Language.Instance.GetMessageFromKey("ASK_BIND")}");
                            return;
                        }
                        else if (DelayUsed)
                        {
                            inventory.ItemInstance.BoundCharacterId = session.Character.CharacterId;
                        }
                    }

                    double timeSpanSinceLastSpUsage = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds - session.Character.LastSp;

                    if (EquipmentSlot == (byte)EquipmentType.Sp && inventory.ItemInstance.Rare == -2)
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_EQUIP_DESTROYED_SP"), 0));
                        return;
                    }

                    if (EquipmentSlot == (byte)EquipmentType.Sp && timeSpanSinceLastSpUsage <= session.Character.SpCooldown && session.Character.EquipmentList.LoadInventoryBySlotAndType((byte)EquipmentType.Sp, InventoryType.Sp) != null)
                    {
                        session.SendPacket(session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage)), 0));
                        return;
                    }
                    if ((ItemType != (byte)Domain.ItemType.Weapon
                         && ItemType != (byte)Domain.ItemType.Armor
                         && ItemType != (byte)Domain.ItemType.Fashion
                         && ItemType != (byte)Domain.ItemType.Jewelery
                         && ItemType != (byte)Domain.ItemType.Specialist)
                        || LevelMinimum > (IsHeroic ? session.Character.HeroLevel : session.Character.Level) || (Sex != 0 && Sex != session.Character.Gender + 1)
                        || ((ItemType != (byte)Domain.ItemType.Jewelery && EquipmentSlot != (byte)EquipmentType.Boots && EquipmentSlot != (byte)EquipmentType.Gloves) && ((Class >> session.Character.Class) & 1) != 1))
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                        return;
                    }

                    if (session.Character.UseSp)
                    {
                        SpecialistInstance sp = session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>(
                                (byte)EquipmentType.Sp,
                                InventoryType.Equipment);

                        if (sp.Item.Element != 0 && EquipmentSlot == (byte)EquipmentType.Fairy && Element != sp.Item.Element && Element != sp.Item.SecondaryElement)
                        {
                            session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                            return;
                        }
                    }

                    if (session.Character.UseSp && EquipmentSlot == (byte)EquipmentType.Sp)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SP_BLOCKED"), 10));
                        return;
                    }

                    if (session.Character.JobLevel < LevelJobMinimum)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 10));
                        return;
                    }

                    Inventory equip = session.Character.EquipmentList.LoadInventoryBySlotAndType(EquipmentSlot, InventoryType.Equipment);
                    if (EquipmentSlot == (byte)EquipmentType.Amulet)
                    {
                        session.SendPacket(session.Character.GenerateEff(39));
                        inventory.ItemInstance.BoundCharacterId = session.Character.CharacterId;
                    }

                    if (equip == null)
                    {
                        session.Character.EquipmentList.AddToInventoryWithSlotAndType(inventory.ItemInstance as ItemInstance, InventoryType.Equipment, EquipmentSlot);
                        session.SendPacket(session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                        session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                        session.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMap?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMap?.Broadcast(session.Character.GeneratePairy());
                    }
                    else
                    {
                        session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                        session.Character.EquipmentList.DeleteFromSlotAndType(equip.Slot, equip.Type);
                        session.SendPacket(session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                        session.Character.EquipmentList.AddToInventoryWithSlotAndType(inventory.ItemInstance as ItemInstance, InventoryType.Equipment, EquipmentSlot);
                        session.Character.InventoryList.AddToInventoryWithSlotAndType(equip.ItemInstance as ItemInstance, type, slot);
                        session.SendPacket(session.Character.GenerateInventoryAdd(equip.ItemInstance.ItemVNum, equip.ItemInstance.Amount, type, slot, equip.ItemInstance.Rare, equip.ItemInstance.Design, equip.ItemInstance.Upgrade, 0));
                        session.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMap?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMap?.Broadcast(session.Character.GeneratePairy());
                    }

                    if (EquipmentSlot == (byte)EquipmentType.Fairy)
                    {
                        WearableInstance fairy = session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Equipment);
                        session.SendPacket(session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("FAIRYSTATS"), fairy.XP, CharacterHelper.LoadFairyXpData((fairy.ElementRate + fairy.Item.ElementRate))), 10));
                    }
                    break;
            }
        }

        #endregion
    }
}