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
using System.Threading;

namespace OpenNos.GameObject
{
    public class FoodItemHandler
    {
        internal void UseItemHandler(Item item, ClientSession session, short effect, int effectValue)
        {

            switch (effect)
            {
                default:
                    Thread workerThread = new Thread(() => regen(session, item));
                    workerThread.Start();
                    break;
            }

        }
        public void regen(ClientSession session, Item item)
        {
            session.Character.IsSitting = true;
            ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateRest(), ReceiverType.AllOnMap);
            session.Client.SendPacket(session.Character.GenerateEff(6000));
            for (int i = 0; i < 5 && session.Character.IsSitting; i++)
            {
                session.Character.Mp += item.Mp / 5;
                session.Character.Hp += item.Hp / 5;
                session.Client.SendPacket(session.Character.GenerateRc(item.Hp / 5));
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateRc(item.Hp / 5), ReceiverType.AllOnMap);
                session.Client.SendPacket(session.Character.GenerateStat());
                Thread.Sleep(1800);
            }
        }



    }
}