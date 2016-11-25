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
using System;
using System.Linq;

namespace OpenNos.GameObject
{
    public class SpecialItem : Item
    {
        #region Instantiation

        public SpecialItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, bool delay = false, string[] packetsplit = null)
        {
            switch (Effect)
            {
                // sp point potions
                case 150:
                case 151:
                    session.Character.SpAdditionPoint += EffectValue;
                    if (session.Character.SpAdditionPoint > 1000000)
                    {
                        session.Character.SpAdditionPoint = 1000000;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;

                case 204:
                    session.Character.SpPoint += EffectValue;
                    session.Character.SpAdditionPoint += EffectValue * 3;
                    if (session.Character.SpAdditionPoint > 1000000)
                    {
                        session.Character.SpAdditionPoint = 1000000;
                    }
                    if (session.Character.SpPoint > 10000)
                    {
                        session.Character.SpPoint = 10000;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;

                // wings
                case 650:
                    SpecialistInstance specialistInstance = session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                    if (session.Character.UseSp && specialistInstance != null)
                    {
                        if (!delay)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_WINGS_CHANGE")}");
                        }
                        else
                        {
                            specialistInstance.Design = (byte)EffectValue;
                            session.Character.MorphUpgrade2 = EffectValue;
                            session.CurrentMap?.Broadcast(session.Character.GenerateCMode());
                            session.SendPacket(session.Character.GenerateStat());
                            session.SendPacket(session.Character.GenerateStatChar());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    }
                    break;

                // presentation messages
                case 203:
                    if (this != null && !session.Character.IsVehicled)
                    {
                        if (!delay)
                        {
                            session.SendPacket(session.Character.GenerateGuri(10, 2, 1));
                        }
                    }
                    break;

                // magic lamps
                case 651:
                    if (!session.Character.Inventory.GetAllItems().Where(i => i.Type == InventoryType.Wear).Any())
                    {
                        if (!delay)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_USE")}");
                        }
                        else
                        {
                            session.Character.ChangeSex();
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                // vehicles
                case 1000:
                    if (Morph > 0)
                    {
                        if (!delay && !session.Character.IsVehicled)
                        {
                            if (session.Character.IsSitting)
                            {
                                session.Character.IsSitting = false;
                                session.CurrentMap?.Broadcast(session.Character.GenerateRest());
                            }
                            session.SendPacket(session.Character.GenerateDelay(3000, 3, $"#u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^2"));
                        }
                        else
                        {
                            if (!session.Character.IsVehicled)
                            {
                                session.Character.Speed = Speed;
                                session.Character.IsVehicled = true;
                                session.Character.VehicleSpeed = Speed;
                                session.Character.MorphUpgrade = 0;
                                session.Character.MorphUpgrade2 = 0;
                                session.Character.Morph = Morph + (byte)session.Character.Gender;
                                session.CurrentMap?.Broadcast(session.Character.GenerateEff(196), session.Character.MapX, session.Character.MapY);
                                session.CurrentMap?.Broadcast(session.Character.GenerateCMode());
                                session.SendPacket(session.Character.GenerateCond());
                            }
                            else
                            {
                                session.Character.RemoveVehicle();
                            }
                        }
                    }
                    break;

                case 69:
                    session.Character.Reput += ReputPrice;
                    session.SendPacket(session.Character.GenerateFd());
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;

                default:
                    Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), this.GetType().ToString()));
                    break;
            }
        }

        #endregion
    }
}