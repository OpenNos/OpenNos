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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.WebApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.Handler
{
    public class MinilandPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public MinilandPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session => _session;

        #endregion

        #region Methods
        /// <summary>
        /// mJoinPacket packet
        /// </summary>
        /// <param name="mJoinPacket"></param>
        public void JoinMiniland(MJoinPacket mJoinPacket)
        {
            ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(mJoinPacket.CharacterId);
            if (sess?.Character != null)
            {
                if (sess?.Character.MinilandState == MinilandState.OPEN)
                {
                    ServerManager.Instance.JoinMiniland(Session, sess);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("MINILAND_CLOSED_BY_FRIEND")));
                }
            }
        }
        public void MinilandRemoveObject(RmvobjPacket packet)
        {
            ItemInstance minilandobject = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (Session.Character.MinilandState == MinilandState.LOCK)
                {
                    MinilandObject mo = Session.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstanceId == minilandobject.Id);
                    if (mo != null)
                    {
                        if (minilandobject.Item.IsMinilandObject)
                        {
                            Session.Character.WareHouseSize = 0;
                        }
                        Session.Character.MinilandObjects.Remove(mo);
                        Session.SendPacket(Session.Character.GenerateMinilandEffect(mo, true));
                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
                        Session.SendPacket(Session.Character.GenerateMinilandObject(mo, packet.Slot, true));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"), 0));
                }
            }
        }
        public void MinilandAddObject(AddobjPacket packet)
        {
            ItemInstance minilandobject = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (!Session.Character.MinilandObjects.Any(s => s.ItemInstanceId == minilandobject.Id))
                {
                    if (Session.Character.MinilandState == MinilandState.LOCK)
                    {
                        MinilandObject mo = new MinilandObject()
                        {
                            CharacterId = Session.Character.CharacterId,
                            ItemInstance = minilandobject,
                            ItemInstanceId = minilandobject.Id,
                            MapX = packet.PositionX,
                            MapY = packet.PositionY,
                            Level1BoxAmount = 0,
                            Level2BoxAmount = 0,
                            Level3BoxAmount = 0,
                            Level4BoxAmount = 0,
                            Level5BoxAmount = 0,
                        };

                        if (minilandobject.Item.ItemType == ItemType.House && Session.Character.MinilandObjects.Any(s => s.ItemInstance.Item.ItemType == ItemType.House && s.ItemInstance.Item.ItemSubType == minilandobject.Item.ItemSubType))
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_THIS_MINILANDOBJECT"), 0));
                            return;
                        }

                        if (minilandobject.Item.IsMinilandObject)
                        {
                            Session.Character.WareHouseSize = minilandobject.Item.MinilandObjectPoint;
                        }
                        Session.Character.MinilandObjects.Add(mo);
                        Session.SendPacket(Session.Character.GenerateMinilandEffect(mo, false));
                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
                        Session.SendPacket(Session.Character.GenerateMinilandObject(mo, packet.Slot, false));


                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"), 0));
                    }
                }
            }
        }

        public void UseMinilandObject(UseobjPacket packet)
        {
            ClientSession client = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Miniland == Session.Character.MapInstance);
            if (client != null)
            {
                ItemInstance minilandobject = client.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
                if (minilandobject != null)
                {
                    MinilandObject mlobj = client.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstanceId == minilandobject.Id);
                    if (mlobj != null)
                    {
                        if (!minilandobject.Item.IsMinilandObject)
                        {
                            bool full = false;
                            Session.SendPacket($"mlo_info {(client == Session ? 1 : 0)} {minilandobject.ItemVNum} {packet.Slot} {Session.Character.MinilandPoint} {(minilandobject.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} 0 999 1000 4999 5000 7999 8000 11999 12000 15999 16000 1000000");
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateStashAll());
                        }
                    }
                }
            }
        }
        public void MinigamePlay(MinigamePacket packet)
        {
            ClientSession client = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Miniland == Session.Character.MapInstance);
            if (client != null)
            {
                MinilandObject mlobj = client.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstance.ItemVNum == packet.MinigameVNum);
                if (mlobj != null)
                {
                    switch (packet.Type)
                    {
                        case 1:
                            int game = ((int)mlobj.ItemInstance.Item.EquipmentSlot == 0) ? 4 + mlobj.ItemInstance.ItemVNum % 10 : (int)mlobj.ItemInstance.Item.EquipmentSlot / 3;
                            Session.SendPacket($"mlo_st {game}");
                            break;
                        case 2:
                            //?
                            break;
                        case 3:
                            Session.SendPacket($"mlo_lv 2");
                            //mg 3 2 3125 8647 8647
                            //mlo_lv 2
                            //eff 1 626114 5102
                            break;
                        case 4:
                            // mg 4 2 3125 2
                            //ivn 2 42.2193.2.0
                            //mlpt 1900 100
                            //mlo_rw 2193 2
                            break;
                        case 5:
                            Session.SendPacket($"mlo_mg {packet.MinigameVNum} {Session.Character.MinilandPoint} 0 0 {mlobj.ItemInstance.Item.MinilandObjectPoint} {mlobj.ItemInstance.Item.MinilandObjectPoint}");
                            break;
                        case 6:
                            //refill
                            break;
                        case 7:
                            //mlo_pmg 3125 2000 0 0 0 0 393 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
                            break;
                        case 8:
                            /*  ivn 2 31.2039.4.0
                                say 1 626114 12 Tu as reçu un objet : Bois normal x 4
                                mlo_pmg 3125 2000 0 0 0 0 0 0 0 0 0 0 0 0 2039 4 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 */
                            break;
                    }
                }
            }
        }
        public void MinilandEdit(MLeditPacket packet)
        {
            switch (packet.Type)
            {
                case 1:
                    Session.SendPacket($"mlintro {packet.Parameters.Replace(' ', '^')}");
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("MINILAND_INFO_CHANGED")));
                    break;
                case 2:
                    MinilandState state;
                    Enum.TryParse(packet.Parameters, out state);

                    switch (state)
                    {
                        case MinilandState.PRIVATE:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_PRIVATE"), 0));
                            //Need to be review to permit one friend limit on the miniland
                            Session.Character.Miniland.Sessions.Where(s => s.Character != Session.Character).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                            break;
                        case MinilandState.LOCK:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_LOCK"), 0));
                            Session.Character.Miniland.Sessions.Where(s => s.Character != Session.Character).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                            break;
                        case MinilandState.OPEN:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_PUBLIC"), 0));
                            break;
                    }

                    Session.Character.MinilandState = state;
                    break;
            }


        }
        #endregion
    }
}