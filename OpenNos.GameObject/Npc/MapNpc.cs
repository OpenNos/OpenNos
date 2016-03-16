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
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapNpc : MapNpcDTO
    {
        #region Instantiation

        public short firstX
        {
            get; set;
        }
        public short firstY
        {
            get; set;
        }


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
        public IEnumerable<TeleporterDTO> Teleporters { get; set; }
        public Shop Shop { get; set; }
        public DateTime LastMove { get; private set; }
        public DateTime LastEffect { get; private set; }
        #endregion

        #region Methods

        public string GetNpcDialog()
        {
                return $"npc_req 2 {MapNpcId} {Dialog}";       
        }

        public string GenerateEInfo()
        {
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc != null)
                return $"e_info 10 {npc.NpcMonsterVNum} {npc.Level} {npc.Element} {npc.AttackClass} {npc.ElementRate} {npc.AttackUpgrade} {npc.DamageMinimum} {npc.DamageMaximum} {npc.Concentrate} {npc.CriticalLuckRate} {npc.CriticalRate} {npc.DefenceUpgrade} {npc.CloseDefence} {npc.DefenceDodge} {npc.DistanceDefence} {npc.DistanceDefenceDodge} {npc.MagicDefence} {npc.FireResistance} {npc.WaterResistance} {npc.LightResistance} {npc.DarkResistance} 0 0 -1 {npc.Name.Replace(' ', '^')}"; // {Hp} {Mp} in 0 0 
            else
                return "";
        }

        public string GenerateEff()
        {
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc != null)
                return $"eff 2 {MapNpcId} {Effect}";
            else
                return "";
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
            time = (DateTime.Now - LastMove).TotalSeconds;
            if (MoveType !=0 && time > 2.5)
            {
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                int oldx = this.MapX;
                int oldy = this.MapY;

                // test.x += (((int)(r.Next(0, 6000)/1000)%2) == 0 )?(-((int)(r.Next(0, 10000)/1000)/2)):((int)(r.Next(0, 10000)/1000)/2);
                // test.y += (((int)(r.Next(0, 6000) / 1000) % 2) == 0) ? (-((int)(r.Next(0, 10000) / 1000) / 2)) : ((int)(r.Next(0, 10000) / 1000) / 2);

                short MapX = (short)r.Next(-2 + this.firstX, 2 + this.firstX);
                short MapY = (short)r.Next(-2 + this.firstY, 2 + this.firstY);
                if (ServerManager.GetMap(MapId).IsBlockedZone(MapX, MapY))
                {
                    this.MapX = MapX;
                    this.MapY = MapY;
                    LastMove = DateTime.Now;

                    string movepacket = $"mv {this.MoveType} {this.MapNpcId} {this.MapX} {this.MapY} {npc.Speed}";
                    ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, movepacket);
                }
            }
        }

        #endregion
    }
}