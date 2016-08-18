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
    public class SpecialItem : Item
    {
        #region Methods

        public override void Use(ClientSession Session, ref Inventory Inv)
        {
            switch (Effect)
            {
                case 650:

                    if (Session.Character.UseSp)
                    {
                        Item iteminfo = ServerManager.GetItem(Inv.ItemInstance.ItemVNum);
                        SpecialistInstance specialistInstance = Session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
                        specialistInstance.Design = (byte)iteminfo.EffectValue;
                        Session.Character.MorphUpgrade2 = iteminfo.EffectValue;
                        Session.Client.SendPacket(Session.Character.GenerateCMode());
                        Session.Client.SendPacket(Session.Character.GenerateStat());
                        Session.Client.SendPacket(Session.Character.GenerateStatChar());
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
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    break;
                default:
                    //Item iteminfo = ServerManager.GetItem(Inv.ItemInstance.ItemVNum);
                    //if (iteminfo.Morph != 0 && iteminfo.Speed != 0)
                    //{
                    //    if (Session.Character.IsVehicled == false)
                    //    {
                    //        Session.Client.SendPacket($"delay 3000 3 #u_i^1^{Session.Character.CharacterId}^{Inv.Type}^{Inv.Slot}^2");
                    //        Session.Character.IsVehicled = true;
                    //        Session.Client.SendPacket("pinit 0");
                    //        Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                    //        // change speed
                    //        Session.Client.SendPacket(Session.Character.GenerateCond());
                    //    }
                    //    else
                    //    {
                    //        Session.Client.SendPacket("pinit 0");
                    //        Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                    //        // revert speed change
                    //        Session.Client.SendPacket(Session.Character.GenerateCond());
                    //    }
                    //}
                    Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), this.GetType().ToString()));
                    break;
            }
        }

        #endregion
    }
}