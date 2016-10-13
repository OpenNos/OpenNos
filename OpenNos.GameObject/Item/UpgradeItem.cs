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
using System;

namespace OpenNos.GameObject
{
    public class UpgradeItem : Item
    {
        #region Methods

        public override void Use(ClientSession Session, ref Inventory Inv, bool DelayUsed = false, string[] packetsplit = null)
        {
            if (EffectValue != 0)
            {
                if (Session.Character.IsSitting)
                {
                    Session.Character.IsSitting = false;
                    Session.SendPacket(Session.Character.GenerateRest());
                }
                Session.SendPacket(Session.Character.GenerateGuri(12, 1, EffectValue));
            }
            else if (EffectValue == 0)
            {
                if (packetsplit != null)
                {
                    byte TypeEquip = 0;
                    short SlotEquip = -1;

                    if (byte.TryParse(packetsplit[8], out TypeEquip) && short.TryParse(packetsplit[9], out SlotEquip))
                    {
                        if (Session.Character.IsSitting)
                        {
                            Session.Character.IsSitting = false;
                            Session.SendPacket(Session.Character.GenerateRest());
                        }
                        if (DelayUsed)
                        {
                            bool isUsed = false;
                            switch (Inv.ItemInstance.ItemVNum)
                            {
                                case 1219:
                                    WearableInstance equip = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(SlotEquip, (Domain.InventoryType)TypeEquip);
                                    if (equip != null && equip.IsFixed)
                                    {
                                        equip.IsFixed = false;
                                        Session.SendPacket(Session.Character.GenerateEff(3003));
                                        Session.CurrentMap?.Broadcast(Session.Character.GenerateGuri(17, 1, SlotEquip));
                                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_UNFIXED"), 12));
                                        isUsed = true;
                                    }
                                    break;

                                case 1365:
                                    SpecialistInstance specialist = Session.Character.InventoryList.LoadBySlotAndType<SpecialistInstance>(SlotEquip, (Domain.InventoryType)TypeEquip);
                                    if (specialist != null && specialist.Rare == -2)
                                    {
                                        specialist.Rare = 0;
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SP_RESURRECTED"), 0));
                                        Session.CurrentMap?.Broadcast(Session.Character.GenerateGuri(13, 1, 1));
                                        Session.Character.SpPoint = 10000;
                                        Session.SendPacket(Session.Character.GenerateSpPoint());
                                        Session.SendPacket(Session.Character.GenerateInventoryAdd(specialist.ItemVNum, 1, (Domain.InventoryType)TypeEquip, SlotEquip, specialist.Rare, specialist.Design, specialist.Upgrade, 0));
                                        isUsed = true;
                                    }
                                    break;
                            }

                            if (!isUsed)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_NOT_FIXED"), 11));
                            }
                            else
                            {
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
                        else
                        {
                            Session.SendPacket($"qna #u_i^1^{Session.Character.CharacterId}^{(byte)Inv.Type}^{Inv.Slot}^0^1^{TypeEquip}^{SlotEquip} {Language.Instance.GetMessageFromKey("QNA_ITEM")}");
                        }
                    }
                }
            }
            else
            {
                Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), this.GetType().ToString()));
            }
        }
    }

    #endregion
}