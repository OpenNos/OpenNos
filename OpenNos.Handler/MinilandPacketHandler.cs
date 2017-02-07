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
                    MapObject mo = Session.Character.Miniland.MapObjects.FirstOrDefault(s => s.ItemInstanceId == minilandobject.Id);
                    if (mo != null)
                    {
                        Session.Character.Miniland.MapObjects.Remove(mo);
                        Session.SendPacket($"eff_g  {minilandobject.Item.EffectValue} {mo.VNum} {mo.MapX} {mo.MapY} 1");
                        Session.SendPacket($"mlpt 2000 100");
                        Session.SendPacket($"mlobj 0 {packet.Slot} {mo.MapX} {mo.MapY} 2 2 0 0 0 0");
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"),0));
                }
            }
        }
        public void MinilandAddObject(AddobjPacket packet)
        {
            ItemInstance minilandobject = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (!Session.Character.Miniland.MapObjects.Any(s => s.ItemInstanceId == minilandobject.Id))
                {
                    if (Session.Character.MinilandState == MinilandState.LOCK)
                    {
                        MapObject mo = new MapObject()
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
                        Session.Character.Miniland.MapObjects.Add(mo);
                        Session.SendPacket($"eff_g  {minilandobject.Item.EffectValue} {mo.VNum} {mo.MapX} {mo.MapY} 0");
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
                            break;
                        case MinilandState.LOCK:
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_LOCK"), 0));
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