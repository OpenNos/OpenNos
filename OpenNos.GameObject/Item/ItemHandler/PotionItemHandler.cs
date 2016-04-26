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

namespace OpenNos.GameObject
{
    public class PotionItemHandler
    {
        #region Methods

        internal void UseItemHandler(ref Inventory inv, ClientSession Session, short effect, int effectValue)
        {
            Item item = ServerManager.GetItem(inv.ItemInstance.ItemVNum);

            switch (effect)
            {
                default:
                    if (Session.Character.Hp == Session.Character.HPLoad() && Session.Character.Mp == Session.Character.MPLoad())
                        return;
                    inv.ItemInstance.Amount--;
                    if (inv.ItemInstance.Amount > 0)
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, 0, 0, 0));
                    else
                    {
                        Session.Character.InventoryList.DeleteFromSlotAndType(inv.
                            Slot, inv.Type);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));
                    }
                    Session.Character.Mp += item.Mp;
                    Session.Character.Hp += item.Hp;
                    if (Session.Character.Mp > Session.Character.MPLoad())
                        Session.Character.Mp = (int)Session.Character.MPLoad();
                    if (Session.Character.Hp > Session.Character.HPLoad())
                        Session.Character.Hp = (int)Session.Character.HPLoad();

                    if (Session.Character.Hp < Session.Character.HPLoad() || Session.Character.Mp < Session.Character.MPLoad())
                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRc(item.Hp), ReceiverType.AllOnMap);
                    Session.Client.SendPacket(Session.Character.GenerateStat());
                    break;
            }
        }

        #endregion
    }
}