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

namespace OpenNos.GameObject
{
    public class UpgradeItem : Item
    {
        public UpgradeItem(ItemDTO item) : base(item)
        {
        }

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            if (Effect == 0)
            {
                if (EffectValue != 0)
                {
                    if (session.Character.IsSitting)
                    {
                        session.Character.IsSitting = false;
                        session.SendPacket(session.Character.GenerateRest());
                    }
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(12, 1, session.Character.CharacterId, EffectValue));
                }
                else if (EffectValue == 0)
                {
                    if (packetsplit != null && packetsplit.Length > 9)
                    {
                        if (byte.TryParse(packetsplit[8], out byte TypeEquip) && short.TryParse(packetsplit[9], out short SlotEquip))
                        {
                            if (session.Character.IsSitting)
                            {
                                session.Character.IsSitting = false;
                                session.SendPacket(session.Character.GenerateRest());
                            }
                            if (Option != 0)
                            {
                                bool isUsed = false;
                                switch (inv.ItemVNum)
                                {
                                    case 1219:
                                        WearableInstance equip = session.Character.Inventory.LoadBySlotAndType<WearableInstance>(SlotEquip, (InventoryType)TypeEquip);
                                        if (equip != null && equip.IsFixed)
                                        {
                                            equip.IsFixed = false;
                                            session.SendPacket(session.Character.GenerateEff(3003));
                                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(17, 1, session.Character.CharacterId, SlotEquip));
                                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_UNFIXED"), 12));
                                            isUsed = true;
                                        }
                                        break;

                                    case 1365:
                                    case 9039:
                                        SpecialistInstance specialist = session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(SlotEquip, (InventoryType)TypeEquip);
                                        if (specialist != null && specialist.Rare == -2)
                                        {
                                            specialist.Rare = 0;
                                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SP_RESURRECTED"), 0));
                                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(13, 1, session.Character.CharacterId, 1));
                                            session.Character.SpPoint = 10000;
                                            if (session.Character.SpPoint > 10000)
                                            {
                                                session.Character.SpPoint = 10000;
                                            }
                                            session.SendPacket(session.Character.GenerateSpPoint());
                                            session.SendPacket(specialist.GenerateInventoryAdd());
                                            isUsed = true;
                                        }
                                        break;
                                }
                                if (!isUsed)
                                {
                                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_NOT_FIXED"), 11));
                                }
                                else
                                {
                                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                                }
                            }
                            else
                            {
                                session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^0^1^{TypeEquip}^{SlotEquip} {Language.Instance.GetMessageFromKey("QNA_ITEM")}");
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
            }
        }
    }

    #endregion
}