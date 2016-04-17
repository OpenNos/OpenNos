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
        private List<MapMonster> _monsters;
        private List<MapNpc> _npcs;
        private List<Portal> _portals;
        private Guid _uniqueIdentifier;

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

            _monsters = new List<MapMonster>();
            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(MapId))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(monster.MonsterVNum);
                _monsters.Add(new MapMonster()
                {
                    MapId = monster.MapId,
                    MapX = monster.MapX,
                    MapMonsterId = monster.MapMonsterId,
                    MapY = monster.MapY,
                    MonsterVNum = monster.MonsterVNum,
                    Position = monster.Position,
                    firstX = monster.MapX,
                    firstY = monster.MapY,
                    IsMoving = monster.IsMoving,
                    Alive = true,
                    CurrentHp = npcmonster.MaxHP,
                    CurrentMp = npcmonster.MaxMP
                });
            }
            IEnumerable<MapNpcDTO> npcsDTO = DAOFactory.MapNpcDAO.LoadFromMap(MapId);

            _npcs = new List<MapNpc>();
            foreach (MapNpcDTO npc in npcsDTO)
            {
                _npcs.Add(new GameObject.MapNpc(npc.MapNpcId)
                {
                    MapId = npc.MapId,
                    MapX = npc.MapX,
                    MapY = npc.MapY,
                    Position = npc.Position,
                    NpcVNum = npc.NpcVNum,
                    IsSitting = npc.IsSitting,
                    IsMoving = npc.IsMoving,
                    Effect = npc.Effect,
                    EffectDelay = npc.EffectDelay,
                    Dialog = npc.Dialog,
                    firstX = npc.MapX,
                    firstY = npc.MapY
                });
            }
        }

        #endregion

        #region Properties

        public IDictionary<long, MapItem> DroppedList { get; set; }

        public int IsDancing { get; set; }

        public List<MapMonster> Monsters
        {
            get
            {
                return _monsters;
            }
        }

        public EventHandler NotifyClients { get; set; }

        public List<MapNpc> Npcs
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

        public int XLength
        {
            get; set;
        }

        public int YLength
        {
            get; set;
        }

        #endregion

        #region Methods

        public List<MapMonster> GetListMonsterInRange(short mapX, short mapY, byte distance)
        {
            List<MapMonster> listmon = new List<MapMonster>();
            foreach (MapMonster mo in Monsters.Where(s => s.Alive))
            {
                if (Math.Pow(mapX - mo.MapX, 2) + Math.Pow(mapY - mo.MapY, 2) <= Math.Pow(distance, 2))
                    listmon.Add(mo);
            }
            return listmon;
        }

        public bool IsBlockedZone(int x, int y)
        {
            if (x < 1 || y < 1 || x > char.MaxValue || y > char.MaxValue || y > _grid.GetLength(0) || x > _grid.GetLength(1) || _grid[y - 1, x - 1] != 0)
            {
                return true;
            }

            return false;
        }

        public bool IsBlockedZone(int firstX, int firstY, int MapX, int MapY)
        {
            for (int i = 1; i <= Math.Abs(MapX - firstX); i++)
            {
                if (IsBlockedZone(firstX + Math.Sign(MapX - firstX) * i, firstY))
                {
                    return true;
                }
            }

            for (int i = 1; i <= Math.Abs(MapY - firstY); i++)
            {
                if (IsBlockedZone(firstX, firstY + Math.Sign(MapY - firstY) * i))
                {
                    return true;
                }
            }
            return false;
        }

        public void ItemSpawn(DropDTO drop, short mapX, short mapY)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int random = 0;
            MapItem DroppedItem = null;
            short MapX = (short)(rnd.Next(mapX - 1, mapX + 1));
            short MapY = (short)(rnd.Next(mapY - 1, mapY + 1));
            while (IsBlockedZone(MapX, MapY))
            {
                MapX = (short)(rnd.Next(mapX - 1, mapX + 1));
                MapY = (short)(rnd.Next(mapY - 1, mapY + 1));
            }

            DroppedItem = new MapItem(MapX, MapY)
            {
                ItemVNum = drop.ItemVNum,
                Amount = (short)drop.Amount,
            };
            while (ServerManager.GetMap(MapId).DroppedList.ContainsKey(random = rnd.Next(1, 999999)))
            { }
            DroppedItem.InventoryItemId = random;
            ServerManager.GetMap(MapId).DroppedList.Add(random, DroppedItem);

            ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, $"drop {DroppedItem.ItemVNum} {random} {DroppedItem.PositionX} {DroppedItem.PositionY} {DroppedItem.Amount} 0 0 -1");
        }

        public void LoadZone()
        {
            Stream stream = new MemoryStream(Data);

            byte[] bytes = new byte[stream.Length];
            int numBytesToRead = 1;
            int numBytesRead = 0;

            byte[] xlength = new byte[2];
            byte[] ylength = new byte[2];
            stream.Read(bytes, numBytesRead, numBytesToRead);
            xlength[0] = bytes[0];
            stream.Read(bytes, numBytesRead, numBytesToRead);
            xlength[1] = bytes[0];
            stream.Read(bytes, numBytesRead, numBytesToRead);
            ylength[0] = bytes[0];
            stream.Read(bytes, numBytesRead, numBytesToRead);
            ylength[1] = bytes[0];

            YLength = BitConverter.ToInt16(ylength, 0);
            XLength = BitConverter.ToInt16(xlength, 0);

            _grid = new char[YLength, XLength];
            for (int i = 0; i < YLength; ++i)
            {
                for (int t = 0; t < XLength; ++t)
                {
                    stream.Read(bytes, numBytesRead, numBytesToRead);
                    _grid[i, t] = Convert.ToChar(bytes[0]);
                }
            }
        }

        public async void MonsterLifeManager()
        {
            var rnd = new Random();
            Task MonsterLifeTask = null;
            foreach (MapMonster monster in Monsters.OrderBy(i => rnd.Next()))
            {
                MonsterLifeTask = new Task(() => monster.MonsterLife());
                MonsterLifeTask.Start();
                await Task.Delay(rnd.Next(1000 / Monsters.Count(), 1000 / Monsters.Count()));
            }
        }

        public async void NpcLifeManager()
        {
            var rnd = new Random();
            Task NpcLifeTask = null;
            foreach (MapNpc npc in Npcs.OrderBy(i => rnd.Next()))
            {
                NpcLifeTask = new Task(() => npc.NpcLife());
                NpcLifeTask.Start();

                await Task.Delay(rnd.Next(1000 / Npcs.Count(), 1000 / Npcs.Count()));
            }
        }

        internal async void MapTaskManager()
        {
            Task NpcMoveTask = new Task(() => NpcLifeManager());
            NpcMoveTask.Start();
            Task MonsterMoveTask = new Task(() => MonsterLifeManager());
            MonsterMoveTask.Start();

            await NpcMoveTask;
            await MonsterMoveTask;
        }

        #endregion
    }
}