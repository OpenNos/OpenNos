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
                        Session.Character.MinilandObjects.Remove(mo);
                        Session.SendPacket(Session.Character.GenerateMinilandEffect(mo, true));
                        Session.SendPacket($"mlpt 2000 100");
                        Session.SendPacket($"mlobj 0 {packet.Slot} {mo.MapX} {mo.MapY} {mo.Item.Width} {mo.Item.Width} 0 0 0 0");
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
                            VNum = minilandobject.ItemVNum,
                            ItemInstanceId = minilandobject.Id,
                            MapX = packet.PositionX,
                            MapY = packet.PositionY,
                            Durability = 0,
                            Level1BoxAmount = 0,
                            Level2BoxAmount = 0,
                            Level3BoxAmount = 0,
                            Level4BoxAmount = 0,
                            Level5BoxAmount = 0,
                        };
                        Session.Character.MinilandObjects.Add(mo);
                        Session.SendPacket(Session.Character.GenerateMinilandEffect(mo, false));
                        Session.SendPacket($"mlpt 2000 100");
                        Session.SendPacket($"mlobj 1 {packet.Slot} {packet.PositionX} {packet.PositionY} 2 2 0 0 0 0");

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
            ClientSession client = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == packet.CharacterName);
            if (client != null)
            {
                ItemInstance minilandobject = client.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
                if (minilandobject != null)
                {
                    MinilandObject mlobj = client.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstanceId == minilandobject.Id);
                    if (mlobj != null)
                    {
                        Session.SendPacket($"mlo_info 1 {minilandobject.ItemVNum} 2 2000 0 0 0 999 1000 4999 5000 7999 8000 11999 12000 15999 16000 1000000");
                        Session.SendPacket($"mg 5 2 {minilandobject.ItemVNum}");
                        Session.SendPacket($"mlo_mg {minilandobject.ItemVNum} 2000 0 0 {minilandobject.DurabilityPoint} {minilandobject.Item.DurabilityPoint}");
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
                        case MinilandState.CLOSED:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_CLOSED"), 0));
                            Session.Character.Miniland.Sessions.Where(s => s.Character != Session.Character).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                            break;
                        case MinilandState.LOCK:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_LOCK"), 0));
                            Session.Character.Miniland.Sessions.Where(s => s.Character != Session.Character).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                            break;
                        case MinilandState.OPEN:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_OPEN"), 0));
                            break;
                    }

                    Session.Character.MinilandState = state;
                    break;
            }


        }
        #endregion
    }
}