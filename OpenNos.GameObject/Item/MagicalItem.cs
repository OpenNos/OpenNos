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
            switch (Effect)
            {
                case 10: //dyes
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (EffectValue == 99)
                            Session.Character.HairColor = (byte)ServerManager.Instance.Random.Next(0, 127);
                        else
                            Session.Character.HairColor = (byte)EffectValue;
                        Session.SendPacket(Session.Character.GenerateEq());
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    break;

                case 11: //waxes
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (Session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                        else
                        {
                            Session.Character.HairStyle = Session.Character.HairStyle != (byte)EffectValue ? (byte)EffectValue : (byte)1;
                            Session.SendPacket(Session.Character.GenerateEq());
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                            Inv.ItemInstance.Amount--;
                            if (Inv.ItemInstance.Amount > 0)
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                        }
                    }
                    break;

                case 14: //Is not good we need to parse it. Asap
                    switch (this.VNum)
                    {
                        case 2156:
                            if (Session.Character.Dignity < 100 && !Session.Character.IsVehicled)
                            {
                                Session.Character.Dignity += 100;

                                if (Session.Character.Dignity > 100) Session.Character.Dignity = 100;

                                Session.SendPacket(Session.Character.GenerateFd());
                                Session.SendPacket(Session.Character.GenerateEff(48));
                                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                                Inv.ItemInstance.Amount--;
                                if (Inv.ItemInstance.Amount > 0)
                                    Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                                else
                                {
                                    Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                                    Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                                }
                            }
                            break;
                    }
                    break;

                case 15: //Speaker
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket(Session.Character.GenerateGuri(10, 3, 1));
                        }
                    }
                    break;

                case 16: //Bubble (Not implemented yet)
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket(Session.Character.GenerateGuri(10, 4, 1));
                        }
                    }
                    break;

                case 30: //wigs
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        WearableInstance wig = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Equipment);
                        if (wig != null)
                        {
                            wig.Design = (byte)ServerManager.Instance.Random.Next(0, 15);
                            Session.SendPacket(Session.Character.GenerateEq());
                            Session.SendPacket(Session.Character.GenerateEquipment());
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                            Inv.ItemInstance.Amount--;
                            if (Inv.ItemInstance.Amount > 0)
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
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

                case 203: //Presentation message
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        if (!DelayUsed)
                        {
                            Session.SendPacket(Session.Character.GenerateGuri(10, 2, 2));
                        }
                    }
                    break;

                case 2168:
                    if (this != null && !Session.Character.IsVehicled)
                    {
                        Session.Character.Dignity = 100;
                        Session.SendPacket(Session.Character.GenerateFd());
                        Session.SendPacket(Session.Character.GenerateEff(48));
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        Session.Character.InventoryList.RemoveItemAmount(this.VNum, 1);
                        if (Inv.ItemInstance.Amount - 1 > 0)
                            Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    break;

                default:
                    Logger.Debug("NO_HANDLER_ITEM");
                    break;
            }
        }

        #endregion
    }
}