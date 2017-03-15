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

using OpenNos.Data;
using System;

namespace OpenNos.GameObject
{
    public class PotionItem : Item
    {
        #region Instantiation

        public PotionItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            if ((DateTime.Now - session.Character.LastPotion).TotalMilliseconds < 750)
            {
                return;
            }
            session.Character.LastPotion = DateTime.Now;
            switch (Effect)
            {
                default:
                    if (session.Character.Hp == session.Character.HPLoad() && session.Character.Mp == session.Character.MPLoad())
                    {
                        return;
                    }
                    if (session.Character.Hp <= 0)
                    {
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    if ((int)session.Character.HPLoad() - session.Character.Hp < Hp)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc((int)session.Character.HPLoad() - session.Character.Hp));
                    }
                    else if ((int)session.Character.HPLoad() - session.Character.Hp > Hp)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(Hp));
                    }
                    session.Character.Mp += Mp;
                    session.Character.Hp += Hp;
                    if (session.Character.Mp > session.Character.MPLoad())
                    {
                        session.Character.Mp = (int)session.Character.MPLoad();
                    }
                    if (session.Character.Hp > session.Character.HPLoad())
                    {
                        session.Character.Hp = (int)session.Character.HPLoad();
                    }
                    if (inv.ItemVNum == 1242 || inv.ItemVNum == 5582)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc((int)session.Character.HPLoad() - session.Character.Hp));
                        session.Character.Hp = (int)session.Character.HPLoad();
                    }
                    else if (inv.ItemVNum == 1243 || inv.ItemVNum == 5583)
                    {
                        session.Character.Mp = (int)session.Character.MPLoad();
                    }
                    else if (inv.ItemVNum == 1244 || inv.ItemVNum == 5584)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc((int)session.Character.HPLoad() - session.Character.Hp));
                        session.Character.Hp = (int)session.Character.HPLoad();
                        session.Character.Mp = (int)session.Character.MPLoad();
                    }
                    session.SendPacket(session.Character.GenerateStat());
                    break;
            }
        }

        #endregion
    }
}