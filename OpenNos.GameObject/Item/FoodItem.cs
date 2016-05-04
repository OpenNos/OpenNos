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
using OpenNos.Core.Networking.Communication.Scs.Communication;
using System;
using System.Threading;

namespace OpenNos.GameObject
{
    public class FoodItem : Item
    {
        #region Methods

        public override void Use(ClientSession Session, ref Inventory Inv)
        {

            Item item = ServerManager.GetItem(Inv.ItemInstance.ItemVNum);
            switch (Effect)
            {
                default:
                    if (Session.Character.IsSitting == false)
                    {
                        Session.Character.SnackAmount = 0;
                        Session.Character.SnackHp = 0;
                        Session.Character.SnackMp = 0;
                    }
                    int amount = Session.Character.SnackAmount;
                    if (amount < 5)
                    {
                        Thread workerThread = new Thread(() => regen(Session, item));
                        workerThread.Start();
                        Inv.ItemInstance.Amount--;
                        if (Inv.ItemInstance.Amount > 0)
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(Inv.ItemInstance.ItemVNum, Inv.ItemInstance.Amount, Inv.Type, Inv.Slot, 0, 0, 0));
                        else
                        {
                            Session.Character.InventoryList.DeleteFromSlotAndType(Inv.Slot, Inv.Type);
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(1, 0, Inv.Type, Inv.Slot, 0, 0, 0));
                        }
                    }
                    else
                    {
                        if (Session.Character.Gender == 1)
                            Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HUNGRY_FEMALE"), 1));
                        else
                            Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HUNGRY_MALE"), 1));
                    }
                    if (amount == 0)
                    {
                        Thread workerThread2 = new Thread(() => sync(Session, item));
                        workerThread2.Start();
                    }
                    break;
            }

        }

        public void regen(ClientSession Session, Item item)
        {
            Session.Character.IsSitting = true;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);

            Session.Client.SendPacket(Session.Character.GenerateEff(6000));
            Session.Character.SnackAmount++;
            Session.Character.MaxSnack = 0;
            Session.Character.SnackHp += item.Hp / 5;
            Session.Character.SnackMp += item.Mp / 5;
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1800);
            }
            Session.Character.SnackHp = item.Hp / 5;
            Session.Character.SnackMp = item.Mp / 5;
            Session.Character.SnackAmount--;
        }

        public void sync(ClientSession Session, Item item)
        {
            for (Session.Character.MaxSnack = 0; Session.Character.MaxSnack < 5 && Session.Character.IsSitting; Session.Character.MaxSnack++)
            {
                Session.Character.Mp += Session.Character.SnackHp;
                Session.Character.Hp += Session.Character.SnackMp;
                if ((Session.Character.SnackHp > 0 && Session.Character.SnackHp > 0) && (Session.Character.Hp < Session.Character.HPLoad() || Session.Character.Mp < Session.Character.MPLoad()))
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRc(Session.Character.SnackHp), ReceiverType.AllOnMap);
                if (Session.Client.CommunicationState == CommunicationStates.Connected)
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateStat(), ReceiverType.OnlyMe);
                else return;
                Thread.Sleep(1800);
            }
        }


        #endregion
    }
}