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

namespace OpenNos.GameObject
{
    public class BoxItem : Item
    {
        #region Instantiation

        public BoxItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, bool delay = false, string[] packetsplit = null)
        {
            switch (Effect)
            {
                case 69:
                    if (EffectValue == 1 || EffectValue == 2)
                    {
                        BoxInstance box = session.Character.Inventory.LoadBySlotAndType<BoxInstance>(inv.Slot, InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"wopen 44 {inv.Slot}");
                            }
                            else
                            {
                                ItemInstance newInv = session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv != null)
                                {
                                    SpecialistInstance specialist = session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(newInv.Slot, newInv.Type);

                                    if (specialist != null)
                                    {
                                        specialist.SlDamage = box.SlDamage;
                                        specialist.SlDefence = box.SlDefence;
                                        specialist.SlElement = box.SlElement;
                                        specialist.SlHP = box.SlHP;
                                        specialist.SpDamage = box.SpDamage;
                                        specialist.SpDark = box.SpDark;
                                        specialist.SpDefence = box.SpDefence;
                                        specialist.SpElement = box.SpElement;
                                        specialist.SpFire = box.SpFire;
                                        specialist.SpHP = box.SpHP;
                                        specialist.SpLevel = box.SpLevel;
                                        specialist.SpLight = box.SpLight;
                                        specialist.SpStoneUpgrade = box.SpStoneUpgrade;
                                        specialist.SpWater = box.SpWater;
                                        specialist.Upgrade = box.Upgrade;
                                        specialist.XP = box.XP;
                                    }

                                    short Slot = inv.Slot;
                                    if (Slot != -1)
                                    {
                                        if (specialist != null)
                                        {
                                            session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {specialist.Item.Name} + {specialist.Upgrade}", 12));
                                            session.SendPacket(session.Character.GenerateInventoryAdd(specialist.ItemVNum, newInv.Amount, specialist.Type, newInv.Slot, 0, 0, specialist.Upgrade, 0));
                                        }
                                        session.Character.Inventory.RemoveItemAmountFromInventory(1, box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }
                    if (EffectValue == 3)
                    {
                        BoxInstance box = session.Character.Inventory.LoadBySlotAndType<BoxInstance>(inv.Slot, InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"guri 26 0 {inv.Slot}");
                            }
                            else
                            {
                                ItemInstance newInv = session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv != null)
                                {
                                    WearableInstance fairy = session.Character.Inventory.LoadBySlotAndType<WearableInstance>(newInv.Slot, newInv.Type);

                                    if (fairy != null)
                                    {
                                        fairy.ElementRate = box.ElementRate;
                                    }

                                    short Slot = inv.Slot;
                                    if (Slot != -1)
                                    {
                                        if (fairy != null)
                                        {
                                            session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {fairy.Item.Name} ({fairy.ElementRate}%)", 12));
                                            session.SendPacket(session.Character.GenerateInventoryAdd(fairy.ItemVNum, newInv.Amount, fairy.Type, newInv.Slot, 0, 0, fairy.Upgrade, 0));
                                        }
                                        session.Character.Inventory.RemoveItemAmountFromInventory(1, box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }
                    if (EffectValue == 4)
                    {
                        BoxInstance box = session.Character.Inventory.LoadBySlotAndType<BoxInstance>(inv.Slot, InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"guri 24 0 {inv.Slot}");
                            }
                            else
                            {
                                ItemInstance newInv = session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv != null)
                                {
                                    short Slot = inv.Slot;
                                    if (Slot != -1)
                                    {
                                        session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newInv.Item.Name} x 1)", 12));
                                        session.SendPacket(session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, 0, 0, newInv.Upgrade, 0));
                                        session.Character.Inventory.RemoveItemAmountFromInventory(1, box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }

                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
                    break;
            }
        }

        #endregion
    }
}