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

using System;
using OpenNos.Data;

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

        public override void Use(ClientSession session, ref ItemInstance inv, bool delay = false, string[] packetsplit = null)
        {
            if ((DateTime.Now - session.Character.LastPotion).TotalMilliseconds < 750)
            {
                return;
            }
            session.Character.LastPotion = DateTime.Now;
            switch (Effect)
            {
                default:
                    if (session.Character.Hp == session.Character.HpLoad() && session.Character.Mp == session.Character.MpLoad())
                    {
                        return;
                    }
                    if (session.Character.Hp <= 0)
                    {
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    if ((int)session.Character.HpLoad() - session.Character.Hp < Hp)
                    {
                        session.CurrentMap?.Broadcast(session.Character.GenerateRc((int)session.Character.HpLoad() - session.Character.Hp));
                    }
                    else if ((int)session.Character.HpLoad() - session.Character.Hp > Hp)
                    {
                        session.CurrentMap?.Broadcast(session.Character.GenerateRc(Hp));
                    }
                    session.Character.Mp += Mp;
                    session.Character.Hp += Hp;
                    if (session.Character.Mp > session.Character.MpLoad())
                    {
                        session.Character.Mp = (int)session.Character.MpLoad();
                    }
                    if (session.Character.Hp > session.Character.HpLoad())
                    {
                        session.Character.Hp = (int)session.Character.HpLoad();
                    }
                    switch (inv.ItemVNum)
                    {
                        case 1242:
                        case 5582:
                            session.CurrentMap?.Broadcast(session.Character.GenerateRc((int)session.Character.HpLoad() - session.Character.Hp));
                            session.Character.Hp = (int)session.Character.HpLoad();
                            break;
                        case 1243:
                        case 5583:
                            session.Character.Mp = (int)session.Character.MpLoad();
                            break;
                        case 1244:
                        case 5584:
                            session.CurrentMap?.Broadcast(session.Character.GenerateRc((int)session.Character.HpLoad() - session.Character.Hp));
                            session.Character.Hp = (int)session.Character.HpLoad();
                            session.Character.Mp = (int)session.Character.MpLoad();
                            break;
                    }
                    session.SendPacket(session.Character.GenerateStat());
                    break;
            }
        }

        #endregion
    }
}