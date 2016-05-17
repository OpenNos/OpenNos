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
    public class PotionItem : Item
    {
        #region Methods

        public override void Use(ClientSession session, ref Inventory inv)
        {
            Item item = ServerManager.GetItem(inv.ItemInstance.ItemVNum);

            switch (Effect)
            {
                default:
                    if (session.Character.Hp == session.Character.HPLoad() && session.Character.Mp == session.Character.MPLoad())
                        return;
                    inv.ItemInstance.Amount--;
                    if (inv.ItemInstance.Amount > 0)
                        session.Client.SendPacket(session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, 0, 0, 0));
                    else
                    {
                        session.Character.InventoryList.DeleteFromSlotAndType(inv.
                            Slot, inv.Type);
                        session.Client.SendPacket(session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));
                    }
                    session.Character.Mp += item.Mp;
                    session.Character.Hp += item.Hp;
                    if (session.Character.Mp > session.Character.MPLoad())
                        session.Character.Mp = (int)session.Character.MPLoad();
                    if (session.Character.Hp > session.Character.HPLoad())
                        session.Character.Hp = (int)session.Character.HPLoad();

                    if (session.Character.Hp < session.Character.HPLoad() || session.Character.Mp < session.Character.MPLoad())
                        session.CurrentMap?.Broadcast(session.Character.GenerateRc(item.Hp));
                    session.Client.SendPacket(session.Character.GenerateStat());
                    break;
            }
        }
    
        #endregion
    }
}