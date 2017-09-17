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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Diagnostics;
using System.Linq;

namespace OpenNos.GameObject
{
    public class WearableItem : Item
    {
        #region Instantiation

        public WearableItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            switch (Effect)
            {
                default:
                    bool delay = false;
                    if (option == 255)
                    {
                        delay = true;
                        option = 0;
                    }
                    Mate mate = null;
                    if (option != 0)
                    {
                        mate = session.Character.Mates.FirstOrDefault(s => s.MateType == MateType.Partner && s.PetId == (option - 1));
                    }
                    short slot = inv.Slot;
                    InventoryType equipment = InventoryType.Wear;
                    switch (option)
                    {
                        case 1:
                            equipment = InventoryType.FirstPartnerInventory;
                            break;

                        case 2:
                            equipment = InventoryType.SecondPartnerInventory;
                            break;

                        case 3:
                            equipment = InventoryType.ThirdPartnerInventory;
                            break;
                    }

                    InventoryType itemToWearType = inv.Type;

                    if (inv == null)
                    {
                        return;
                    }

                    if (ItemValidTime > 0 && inv.IsBound)
                    {
                        inv.ItemDeleteTime = DateTime.Now.AddSeconds(ItemValidTime);
                    }
                    if (!inv.IsBound)
                    {
                        if (!delay && (EquipmentSlot == EquipmentType.Fairy && (MaxElementRate == 70 || MaxElementRate == 80) || EquipmentSlot == EquipmentType.CostumeHat || EquipmentSlot == EquipmentType.CostumeSuit || EquipmentSlot == EquipmentType.WeaponSkin))
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)itemToWearType}^{slot}^1 {Language.Instance.GetMessageFromKey("ASK_BIND")}");
                            return;
                        }
                        if (delay)
                        {
                            inv.BoundCharacterId = session.Character.CharacterId;
                        }
                    }

                    double timeSpanSinceLastSpUsage = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds - session.Character.LastSp;

                    if (EquipmentSlot == EquipmentType.Sp && inv.Rare == -2)
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_EQUIP_DESTROYED_SP"), 0));
                        return;
                    }

                    if (option == 0)
                    {
                        if (EquipmentSlot == EquipmentType.Sp && timeSpanSinceLastSpUsage <= session.Character.SpCooldown && session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Specialist) != null)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage)), 0));
                            return;
                        }

                        if (ItemType != ItemType.Weapon && ItemType != ItemType.Armor && ItemType != ItemType.Fashion && ItemType != ItemType.Jewelery && ItemType != ItemType.Specialist ||
                            LevelMinimum > (IsHeroic ? session.Character.HeroLevel : session.Character.Level) || Sex != 0 && Sex != (byte)session.Character.Gender + 1
                            || ItemType != ItemType.Jewelery && EquipmentSlot != EquipmentType.Boots && EquipmentSlot != EquipmentType.Gloves && ((Class >> (byte)session.Character.Class) & 1) != 1)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                            return;
                        }

                        if (session.Character.UseSp)
                        {
                            SpecialistInstance sp = session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, equipment);

                            if (sp != null && sp.Item.Element != 0 && EquipmentSlot == EquipmentType.Fairy && Element != sp.Item.Element && Element != sp.Item.SecondaryElement)
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                                return;
                            }

                        }

                        if (session.Character.UseSp && EquipmentSlot == EquipmentType.Sp)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SP_BLOCKED"), 10));
                            return;
                        }

                        if (session.Character.JobLevel < LevelJobMinimum)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 10));
                            return;
                        }
                    }
                    else if (mate != null)
                    {
                        if (mate.Level < LevelMinimum)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                            return;
                        }
                        switch (EquipmentSlot)
                        {
                            case EquipmentType.Armor:
                                if (ItemSubType == 4)
                                {
                                    mate.ArmorInstance = inv;
                                    break;
                                }
                                else
                                {
                                    goto default;
                                }

                            case EquipmentType.MainWeapon:
                                if (ItemSubType == 12)
                                {
                                    mate.WeaponInstance = inv;
                                    break;
                                }
                                else
                                {
                                    goto default;
                                }

                            case EquipmentType.Gloves:
                                mate.GlovesInstance = inv;
                                break;

                            case EquipmentType.Boots:
                                mate.BootsInstance = inv;
                                break;

                            case EquipmentType.Sp:
                                if (ItemSubType == 4)
                                {
                                    mate.SpInstance = inv;
                                    break;
                                }
                                else
                                {
                                    goto default;
                                }

                            default:
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                                return;
                        }
                    }

                    ItemInstance currentlyEquippedItem = session.Character.Inventory.LoadBySlotAndType((short)EquipmentSlot, equipment);

                    if (currentlyEquippedItem == null)
                    {
                        // move from equipment to wear
                        session.Character.Inventory.MoveInInventory(inv.Slot, itemToWearType, equipment);
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(itemToWearType, slot));
                    }
                    else
                    {
                        // move from wear to equipment and back
                        session.Character.Inventory.MoveInInventory(currentlyEquippedItem.Slot, equipment, itemToWearType, inv.Slot);
                        session.SendPacket(currentlyEquippedItem.GenerateInventoryAdd());
                        session.Character.EquipmentBCards = session.Character.EquipmentBCards.Where(o => o.ItemVNum != currentlyEquippedItem.ItemVNum);
                        switch (currentlyEquippedItem.Slot)
                        {
                            case (byte) EquipmentType.Armor:
                                session.Character.Inventory.Armor = null;
                                break;
                            case (byte) EquipmentType.MainWeapon:
                                session.Character.Inventory.PrimaryWeapon = null;
                                break;
                            case (byte) EquipmentType.SecondaryWeapon:
                                session.Character.Inventory.SecondaryWeapon = null;
                                break;
                        }
                    }
                    inv.Item.BCards.ForEach(s => session.Character.EquipmentBCards.Add(s));

                    if (inv is WearableInstance wearableInstance)
                    {
                        if (wearableInstance.EquipmentOptions != null)
                        {
                            switch (wearableInstance.Item.ItemType)
                            {
                                case ItemType.Armor:
                                case ItemType.Weapon:
                                    switch (wearableInstance.Slot)
                                    {
                                        case (byte)EquipmentType.Armor:
                                            session.Character.Inventory.Armor = wearableInstance;
                                            break;
                                        case (byte)EquipmentType.MainWeapon:
                                            session.Character.Inventory.PrimaryWeapon = wearableInstance;
                                            break;
                                        case (byte)EquipmentType.SecondaryWeapon:
                                            session.Character.Inventory.SecondaryWeapon = wearableInstance;
                                            break;
                                    }
                                    EquipmentOptionHelper.Instance.ShellToBCards(wearableInstance.EquipmentOptions, wearableInstance.ItemVNum).ForEach(s => session.Character.EquipmentBCards.Add(s));
                                    break;
                                case ItemType.Jewelery:
                                    EquipmentOptionHelper.Instance.CellonToBCards(wearableInstance.EquipmentOptions, wearableInstance.ItemVNum).ForEach(s => session.Character.EquipmentBCards.Add(s));
                                    break;
                            }

                        }
                    }

                    if (option == 0)
                    {
                        session.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMapInstance?.Broadcast(session.Character.GeneratePairy());

                        switch (EquipmentSlot)
                        {
                            case EquipmentType.Fairy:
                                WearableInstance fairy = session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, equipment);
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("FAIRYSTATS"), fairy.XP, CharacterHelper.LoadFairyXpData(fairy.ElementRate + fairy.Item.ElementRate)), 10));
                                break;
                            case EquipmentType.Amulet:
                                session.SendPacket(session.Character.GenerateEff(39));
                                inv.BoundCharacterId = session.Character.CharacterId;
                                break;
                        }
                    }
                    else if (mate != null)
                    {
                        session.SendPacket(mate.GenerateScPacket());
                    }
                    break;
            }
        }

        #endregion
    }
}