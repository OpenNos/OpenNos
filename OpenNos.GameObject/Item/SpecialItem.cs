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
using System.Linq;

namespace OpenNos.GameObject
{
    public class SpecialItem : Item
    {
        #region Methods

        public override void Use(ClientSession Session, ref Inventory inventory, bool DelayUsed = false)
        {
            switch (Effect)
            {
                // wings
                case 650:
                    SpecialistInstance specialistInstance = Session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Equipment);
                    if (Session.Character.UseSp && specialistInstance != null)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket($"qna #u_i^1^{Session.Character.CharacterId}^{(byte)inventory.Type}^{inventory.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_WINGS_CHANGE")}");
                        }
                        else
                        {
                            specialistInstance.Design = (byte)EffectValue;
                            Session.Character.MorphUpgrade2 = EffectValue;
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                            Session.SendPacket(Session.Character.GenerateStat());
                            Session.SendPacket(Session.Character.GenerateStatChar());

                            inventory.ItemInstance.Amount--;
                            if (inventory.ItemInstance.Amount > 0)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(inventory.ItemInstance.ItemVNum, inventory.ItemInstance.Amount, inventory.Type, inventory.Slot, 0, 0, 0, 0));
                            }
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inventory.Type, inventory.Slot, 0, 0, 0, 0));
                            }
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    }
                    break;

                // magic lamps
                case 651:
                    if (!Session.Character.EquipmentList.Inventory.Any())
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket($"qna #u_i^1^{Session.Character.CharacterId}^{(byte)inventory.Type}^{inventory.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_USE")}");
                        }
                        else
                        {
                            Session.Character.ChangeSex();
                            inventory.ItemInstance.Amount--;
                            if (inventory.ItemInstance.Amount > 0)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(inventory.ItemInstance.ItemVNum, inventory.ItemInstance.Amount, inventory.Type, inventory.Slot, 0, 0, 0, 0));
                            }
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inventory.Type, inventory.Slot, 0, 0, 0, 0));
                            }
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                // vehicles
                case 1000:
                    SpecialistInstance sp = Session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Equipment);
                    if (!DelayUsed && !Session.Character.IsVehicled)
                    {
                        if (Session.Character.IsSitting)
                        {
                            Session.Character.IsSitting = false;
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateRest());
                        }
                        Session.SendPacket(Session.Character.GenerateDelay(3000, 3, $"#u_i^1^{Session.Character.CharacterId}^{(byte)inventory.Type}^{inventory.Slot}^2"));
                    }
                    else
                    {
                        if (!Session.Character.IsVehicled)
                        {
                            Session.Character.IsVehicled = true;
                            Session.Character.MorphUpgrade = 0;
                            Session.Character.MorphUpgrade2 = 0;
                            Session.Character.Morph = Morph + Session.Character.Gender;
                            Session.Character.Speed = Speed;
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(196));
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                            Session.SendPacket(Session.Character.GenerateCond());
                        }
                        else
                        {
                            Session.Character.RemoveVehicle();
                        }
                    }
                    break;

                default:
                    Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), this.GetType().ToString()));
                    break;
            }
        }

        #endregion
    }
}