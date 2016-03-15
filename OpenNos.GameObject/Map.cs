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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Map : MapDTO
    {
        #region Members

        private char[,] _grid;
        private List<Npc> _npcs;
        private List<Portal> _portals;
        private Guid _uniqueIdentifier;
        private int _xLength;
        private int _yLength;

        #endregion

        #region Instantiation

        public Map(short mapId, Guid uniqueIdentifier, byte[] data)
        {
            Mapper.CreateMap<MapDTO, Map>();
            Mapper.CreateMap<Map, MapDTO>();

            MapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            Data = data;
            LoadZone();
            IEnumerable<PortalDTO> portalsDTO = DAOFactory.PortalDAO.LoadByMap(MapId);
            _portals = new List<Portal>();
            DroppedList = new Dictionary<long, MapItem>();

            ShopUserList = new Dictionary<long, MapShop>();
            foreach (PortalDTO portal in portalsDTO)
            {
                _portals.Add(new GameObject.Portal()
                {
                    DestinationMapId = portal.DestinationMapId,
                    SourceMapId = portal.SourceMapId,
                    SourceX = portal.SourceX,
                    SourceY = portal.SourceY,
                    DestinationX = portal.DestinationX,
                    DestinationY = portal.DestinationY,
                    Type = portal.Type,
                    PortalId = portal.PortalId,
                    IsDisabled = portal.IsDisabled
                });
            }
            IEnumerable<NpcDTO> npcsDTO = DAOFactory.NpcDAO.LoadFromMap(MapId);
            _npcs = new List<Npc>();
            foreach (NpcDTO npc in npcsDTO)
            {
                _npcs.Add(new GameObject.Npc(npc.NpcId)
                {
                    Dialog = npc.Dialog,
                    MapId = npc.MapId,
                    MapX = npc.MapX,
                    MapY = npc.MapY,
                    Name = npc.Name,
                    Level = npc.Level,
                    Position = npc.Position,
                    Vnum = npc.Vnum,
                    AttackClass = npc.AttackClass,
                    AttackUpgrade = npc.AttackUpgrade,
                    CloseDefence = npc.CloseDefence,
                    Concentrate = npc.Concentrate,
                    CriticalLuckRate = npc.CriticalLuckRate,
                    CriticalRate = npc.CriticalRate,
                    DamageMaximum = npc.DamageMaximum,
                    DamageMinimum = npc.DamageMinimum,
                    DarkResistance = npc.DarkResistance,
                    DefenceDodge = npc.DefenceDodge,
                    DefenceUpgrade = npc.DefenceUpgrade,
                    DistanceDefence = npc.DistanceDefence,
                    DistanceDefenceDodge = npc.DistanceDefenceDodge,
                    Effect = npc.Effect,
                    EffectDelay = npc.EffectDelay,
                    Element = npc.Element,
                    ElementRate = npc.ElementRate,
                    FireResistance = npc.FireResistance,
                    IsSitting = npc.IsSitting,
                    LightResistance = npc.LightResistance,
                    MagicDefence = npc.MagicDefence,
                    Move = npc.Move,
                    NpcId = npc.NpcId,
                    Speed = npc.Speed,
                    WaterResistance = npc.WaterResistance,
                    firstX = npc.MapX,
                    firstY = npc.MapY
                });
            }
        }
       public async void NpcMove()
        {
            var rnd = new Random();
            Task NpcLifeTask = null;
            foreach (Npc npc in Npcs.OrderBy(i => rnd.Next()))
            {
                NpcLifeTask = new Task(() => npc.NpcLife());
                NpcLifeTask.Start();

                await Task.Delay(300);
            }
          
        }
        internal async void MapTaskManager()
        {
            Task NpcMoveTask = new Task(() => NpcMove());
            NpcMoveTask.Start();
            await NpcMoveTask;
        }

        #endregion

        #region Properties

        public IDictionary<long, MapItem> DroppedList { get; set; }

        public int IsDancing { get; set; }

        public EventHandler NotifyClients { get; set; }

        public List<Npc> Npcs
        {
            get
            {
                return _npcs;
            }
        }

        public List<Portal> Portals
        {
            get
            {
                return _portals;
            }
        }

        public Dictionary<long, MapShop> ShopUserList { get; set; }

        #endregion

        #region Methods

        public bool IsBlockedZone(int x, int y)
        {
            if (x > _xLength || x < 1 || y > _yLength || y < 1 || _grid[y - 1, x - 1] == 1)
            {
                return true;
            }

            return false;
        }

        public void LoadZone()
        {
            Stream stream = new MemoryStream(Data);

            byte[] bytes = new byte[stream.Length];
            int numBytesToRead = 1;
            int numBytesRead = 0;

            stream.Read(bytes, numBytesRead, numBytesToRead);
            _xLength = bytes[0];
            stream.Read(bytes, numBytesRead, numBytesToRead);
            stream.Read(bytes, numBytesRead, numBytesToRead);
            _yLength = bytes[0];
            stream.Read(bytes, numBytesRead, numBytesToRead);
            _grid = new char[_yLength, _xLength];
            for (int i = 0; i < _yLength; ++i)
            {
                for (int t = 0; t < _xLength; ++t)
                {
                    stream.Read(bytes, numBytesRead, numBytesToRead);
                    _grid[i, t] = Convert.ToChar(bytes[0]);
                }
            }
        }

        #endregion
    }
}