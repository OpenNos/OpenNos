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

using AutoMapper;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class MapNpc : MapNpcDTO
    {
        #region Instantiation

        public MapNpc(int npcId)
        {
            Mapper.CreateMap<MapNpcDTO, MapNpc>();
            Mapper.CreateMap<MapNpc, MapNpcDTO>();
            MapNpcId = npcId;
            LastEffect = LastMove = DateTime.Now;
            IEnumerable<TeleporterDTO> Teleporters = DAOFactory.TeleporterDAO.LoadFromNpc(MapNpcId);
            ShopDTO shop = DAOFactory.ShopDAO.LoadByNpc(MapNpcId);
            if (shop != null)
                Shop = new Shop(shop.ShopId) { Name = shop.Name, MapNpcId = MapNpcId, MenuType = shop.MenuType, ShopType = shop.ShopType };
        }

        #endregion

        #region Properties

        public short firstX { get; set; }
        public short firstY { get; set; }
        public DateTime LastEffect { get; private set; }
        public DateTime LastMove { get; private set; }
        public Shop Shop { get; set; }
        public IEnumerable<TeleporterDTO> Teleporters { get; set; }

        #endregion

        #region Methods

        public string GenerateEff()
        {
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc != null)
                return $"eff 2 {MapNpcId} {Effect}";
            else
                return "";
        }

        public string GetNpcDialog()
        {
            return $"npc_req 2 {MapNpcId} {Dialog}";
        }

        internal void NpcLife()
        {
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc == null)
                return;
            double time = (DateTime.Now - LastEffect).TotalMilliseconds;
            if (Effect > 0 && time > EffectDelay)
            {
                ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, GenerateEff());
                LastEffect = DateTime.Now;
            }

    
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
             time = (DateTime.Now - LastMove).TotalSeconds;
            if (Move && time > r.Next(1, 2) * (1 + r.NextDouble()))
            {
                byte point = (byte)r.Next(2, 5);
                byte fpoint = (byte)r.Next(0, 2);

                byte xpoint = (byte)r.Next(fpoint, point);
                byte ypoint = (byte)(point - xpoint);

                short MapX = (short)r.Next(-xpoint + firstX, xpoint + firstX);
                short MapY = (short)r.Next(-ypoint + firstY, ypoint + firstY);
                if (!ServerManager.GetMap(MapId).IsBlockedZone(firstX, firstY, MapX, MapY))
                {
                    this.MapX = MapX;
                    this.MapY = MapY;
                    LastMove = DateTime.Now;

                    string movepacket = $"mv 2 {this.MapNpcId} {this.MapX} {this.MapY} {npc.Speed}";
                    ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, movepacket);
                }
            }
        }

        #endregion
    }
}