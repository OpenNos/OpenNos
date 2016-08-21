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

        public override void Use(ClientSession Session, ref Inventory Inv)
        {
            Item iteminfo = ServerManager.GetItem(Inv.ItemInstance.ItemVNum);
            Random rnd = new Random();
            switch (Effect)
            {
                case 10:
                    if (iteminfo != null)
                    {
                        if (EffectValue == 99)
                            Session.Character.HairColor = (byte)rnd.Next(0, 127);
                        else
                            Session.Character.HairColor = (byte)EffectValue;
                        Session.Client.SendPacket(Session.Character.GenerateEq());
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                        }
                    }
                    break;
                case 11:
                    if (iteminfo != null)
                    {
                        if (Session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                        else
                        {
                            Session.Character.HairStyle = Session.Character.HairStyle != (byte)EffectValue ? (byte)EffectValue : (byte)1;
                            Session.Client.SendPacket(Session.Character.GenerateEq());
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                            Inv.ItemInstance.Amount--;
                            if (Inv.ItemInstance.Amount > 0)
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                        }
                    }
                    break;

                case 30:
                    if (iteminfo != null)
                    {
                        WearableInstance wig = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, (byte)InventoryType.Equipment);
                        if (wig != null)
                        {
                            wig.Design = (byte)rnd.Next(0, 15);
                            Session.Client.SendPacket(Session.Character.GenerateEq());
                            Session.Client.SendPacket(Session.Character.GenerateEquipment());
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.All);
                            Inv.ItemInstance.Amount--;
                            if (Inv.ItemInstance.Amount > 0)
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            else
                            {
                                Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, Inv.Type, Inv.Slot, 0, 0, 0, 0));
                            }
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                            return;
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