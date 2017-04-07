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
using System;
using System.Threading;

namespace OpenNos.GameObject
{
    public class SnackItem : Item
    {
        #region Instantiation

        public SnackItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public void Regenerate(ClientSession session, Item item, string[] packetsplit = null)
        {
            session.SendPacket(session.Character.GenerateEff(6000));
            session.Character.SnackAmount++;
            session.Character.MaxSnack = 0;
            session.Character.SnackHp += item.Hp / 5;
            session.Character.SnackMp += item.Mp / 5;
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1800);
            }
            session.Character.SnackHp -= item.Hp / 5;
            session.Character.SnackMp -= item.Mp / 5;
            session.Character.SnackAmount--;
        }

        public void Sync(ClientSession session, Item item)
        {
            for (session.Character.MaxSnack = 0; session.Character.MaxSnack < 5; session.Character.MaxSnack++)
            {
                if (session.Character.Hp <= 0)
                {
                    return;
                }
                session.Character.Hp += session.Character.SnackHp;
                session.Character.Mp += session.Character.SnackMp;
                if (session.Character.Mp > session.Character.MPLoad())
                {
                    session.Character.Mp = (int)session.Character.MPLoad();
                }
                if (session.Character.Hp > session.Character.HPLoad())
                {
                    session.Character.Hp = (int)session.Character.HPLoad();
                }
                if (session.Character.Hp < session.Character.HPLoad() || session.Character.Mp < session.Character.MPLoad())
                {
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRc(session.Character.SnackHp));
                }
                if (session.IsConnected)
                {
                    session.SendPacket(session.Character.GenerateStat());
                }
                else
                {
                    return;
                }
                Thread.Sleep(1800);
            }
        }

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            if ((DateTime.Now - session.Character.LastPotion).TotalMilliseconds < 750)
            {
                return;
            }
            session.Character.LastPotion = DateTime.Now;
            Item item = inv.Item;
            switch (Effect)
            {
                default:
                    if (session.Character.Hp <= 0)
                    {
                        return;
                    }
                    int amount = session.Character.SnackAmount;
                    if (amount < 5)
                    {
                        Thread workerThread = new Thread(() => Regenerate(session, item));
                        workerThread.Start();
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    else
                    {
                        session.SendPacket(session.Character.Gender == GenderType.Female
                            ? session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HUNGRY_FEMALE"), 1)
                            : session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HUNGRY_MALE"), 1));
                    }
                    if (amount == 0)
                    {
                        Thread workerThread2 = new Thread(() => Sync(session, item));
                        workerThread2.Start();
                    }
                    break;
            }
        }

        #endregion
    }
}