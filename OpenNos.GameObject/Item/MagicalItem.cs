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

namespace OpenNos.GameObject
{
    public class MagicalItem : Item
    {
        #region Methods
        public override void Use(ClientSession Session, ref Inventory Inv, bool DelayUsed = false)
        {
            Random random = new Random();
            switch (Effect)
            {
                // airwaves
                case 0:
                    if (this != null && this.ItemType == (byte)Domain.ItemType.Event)
                    {
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(EffectValue));
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                        {
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    break;

                // dyes
                case 10:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (EffectValue == 99)
                        {
                            Session.Character.HairColor = (byte)random.Next(0, 127);
                        }
                        else
                        {
                            Session.Character.HairColor = (byte)EffectValue;
                        }
                        Session.SendPacket(Session.Character.GenerateEq());
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                        {
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    break;

                // waxes
                case 11:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (Session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                        }
                        else
                        {
                            Session.Character.HairStyle = Session.Character.HairStyle != (byte)EffectValue ? (byte)EffectValue : (byte)1;
                            Session.SendPacket(Session.Character.GenerateEq());
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                            Inv.ItemInstance.Amount--;
                            if (Inv.ItemInstance.Amount > 0)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                        }
                    }
                    break;

                // dignity restoration
                case 14:
                    if ((EffectValue == 100 || EffectValue == 200) && Session.Character.Dignity < 100 && !Session.Character.IsVehicled)
                    {
                        Session.Character.Dignity += EffectValue;
                        if (Session.Character.Dignity > 100)
                        {
                            Session.Character.Dignity = 100;
                        }
                        Session.SendPacket(Session.Character.GenerateFd());
                        Session.SendPacket(Session.Character.GenerateEff(48));
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                        {
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    else if (EffectValue == 2000 && Session.Character.Dignity < 100 && !Session.Character.IsVehicled)
                    {
                        Session.Character.Dignity = 100;
                        Session.SendPacket(Session.Character.GenerateFd());
                        Session.SendPacket(Session.Character.GenerateEff(48));
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                        {
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    break;

                // speakers
                case 15:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket(Session.Character.GenerateGuri(10, 3, 1));
                        }
                    }
                    break;

                // bubbles
                case 16:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket(Session.Character.GenerateGuri(10, 4, 1));
                        }
                    }
                    break;

                // wigs
                case 30:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        WearableInstance wig = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Equipment);
                        if (wig != null)
                        {
                            wig.Design = (byte)random.Next(0, 15);
                            Session.SendPacket(Session.Character.GenerateEq());
                            Session.SendPacket(Session.Character.GenerateEquipment());
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                            Inv.ItemInstance.Amount--;
                            if (Inv.ItemInstance.Amount > 0)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                            return;
                        }
                    }
                    break;

                // presentation messages
                case 203:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket(Session.Character.GenerateGuri(10, 2, 2));
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