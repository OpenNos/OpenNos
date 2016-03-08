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
using System.Threading;

namespace OpenNos.GameObject
{
    public class SnackItemHandler
    {
        internal void UseItemHandler(Item item, ClientSession session, short effect, int effectValue)
        {
            switch (effect)
            {
                default:
                    if (session.Character.SnackAmount < 5)
                    {
                        Thread workerThread = new Thread(() => regen(session, item));
                        workerThread.Start();
                    }
                    else
                    {
                        session.Client.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HANGRY"), 1));
                    }
                    if (session.Character.SnackAmount == 1)
                    {
                        Thread workerThread2 = new Thread(() => sync(session, item));
                        workerThread2.Start();
                    }
                        break;
            }

        }
        public void regen(ClientSession session, Item item)
        {
            session.Client.SendPacket(session.Character.GenerateEff(6000));
            session.Character.SnackAmount++;
            session.Character.MaxSnack=0;
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
        public void sync(ClientSession session, Item item)
        {
            for (session.Character.MaxSnack = 0; session.Character.MaxSnack < 5; session.Character.MaxSnack++)
            {
                session.Character.Mp += session.Character.SnackHp;
                session.Character.Hp += session.Character.SnackMp;
                if (session.Character.Hp < session.Character.HPLoad() || session.Character.Mp < session.Character.MPLoad())
                    ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateRc(session.Character.SnackHp), ReceiverType.AllOnMap);
                session.Client.SendPacket(session.Character.GenerateStat());
                Thread.Sleep(1800);
            }
        }
    }
}