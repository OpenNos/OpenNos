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

using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Map : BroadcastableBase, IMapDTO
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
            MapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            Data = data;
            LoadZone();
            IEnumerable<PortalDTO> portalsDTO = DAOFactory.PortalDAO.LoadByMap(MapId);
            _portals = new List<Portal>();
            DroppedList = new Dictionary<long, MapItem>();

            MapTypes = new List<MapType>();
            foreach (MapTypeMapDTO maptypemap in DAOFactory.MapTypeMapDAO.LoadByMapId(mapId))
            {
                MapTypeDTO MT = DAOFactory.MapTypeDAO.LoadById(maptypemap.MapTypeId);
                MapType maptype = new MapType()
                {
                    MapTypeId = MT.MapTypeId,
                    MapTypeName = MT.MapTypeName,
                    PotionDelay = MT.PotionDelay
                };
                MapTypes.Add(maptype);
            }

            UserShops = new Dictionary<long, MapShop>();
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
            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(MapId).ToList())
            {
                NpcMonster npcmonster = ServerManager.GetNpc(monster.MonsterVNum);
                _monsters.Add(new MapMonster(this, monster.MonsterVNum)
                {
                    MapId = monster.MapId,
                    MapX = monster.MapX,
                    MapMonsterId = monster.MapMonsterId,
                    MapY = monster.MapY,
                    Position = monster.Position,
                    firstX = monster.MapX,
                    firstY = monster.MapY,
                    IsMoving = monster.IsMoving,
                    Alive = true,
                    CurrentHp = npcmonster.MaxHP,
                    CurrentMp = npcmonster.MaxMP
                });
            }
            IEnumerable<MapNpcDTO> npcsDTO = DAOFactory.MapNpcDAO.LoadFromMap(MapId).ToList();

            _npcs = new List<MapNpc>();
            foreach (MapNpcDTO npc in npcsDTO)
            {
                _npcs.Add(new GameObject.MapNpc(npc.MapNpcId, this)
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
                    FirstX = npc.MapX,
                    FirstY = npc.MapY
                });
            }
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        public IDictionary<long, MapItem> DroppedList { get; set; }

        public int IsDancing { get; set; }

        public short MapId { get; set; }

        public List<MapType> MapTypes
        {
            get; set;
        }

        public List<MapMonster> Monsters
        {
            get
            {
                return _monsters;
            }
        }

        public int Music { get; set; }

        public string Name { get; set; }

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

        public Dictionary<long, MapShop> UserShops { get; set; }

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

        public static int GetDistance(MapCell p, MapCell q)
        {
            double a = p.X - q.X;
            double b = p.Y - q.Y;

            if (a * a == b * b)
                a = 0;
            double distance = Math.Sqrt(a * a + b * b);
            return (int)distance;
        }

        public List<MapCell> AStar(MapCell cell1, MapCell cell2)
        {
            List<MapCell> SolutionPathList = new List<MapCell>();

            SortedCostMapCellList OPEN = new SortedCostMapCellList();
            SortedCostMapCellList CLOSED = new SortedCostMapCellList();
            MapCellAStar cell_start = new MapCellAStar(null, null, cell1.X, cell1.Y, cell1.MapId);
            MapCellAStar cell_goal = new MapCellAStar(null, null, cell2.X, cell2.Y, cell2.MapId);
            OPEN.Push(cell_start);
            if (cell1.MapId != cell2.MapId)
            {
                SolutionPathList.Insert(0, cell_start);
                return SolutionPathList;
            }
            while (OPEN.Count > 0)
            {
                MapCellAStar cell_current = OPEN.Pop();

                if (cell_current.IsMatch(cell_goal))
                {
                    cell_goal.parentcell = cell_current.parentcell;
                    break;
                }

                List<MapCellAStar> successors = cell_current.GetSuccessors();

                foreach (MapCellAStar cell_successor in successors)
                {
                    int oFound = OPEN.IndexOf(cell_successor);

                    if (oFound > 0)
                    {
                        MapCellAStar existing_cell = OPEN.CellAt(oFound);
                        if (existing_cell.CompareTo(cell_current) <= 0)
                            continue;
                    }

                    int cFound = CLOSED.IndexOf(cell_successor);

                    if (cFound > 0)
                    {
                        MapCellAStar existing_cell = CLOSED.CellAt(cFound);
                        if (existing_cell.CompareTo(cell_current) <= 0)
                            continue;
                    }

                    if (oFound != -1)
                        OPEN.RemoveAt(oFound);
                    if (cFound != -1)
                        CLOSED.RemoveAt(cFound);

                    OPEN.Push(cell_successor);
                }
                CLOSED.Push(cell_current);
            }
            MapCellAStar p = cell_goal;
            while (p != null)
            {
                SolutionPathList.Insert(0, p);
                p = p.parentcell;
            }
            return SolutionPathList;
        }

        public void DropItemByMonster(DropDTO drop, short mapX, short mapY)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int random = 0;
            MapItem droppedItem = null;
            short MapX = (short)(rnd.Next(mapX - 1, mapX + 1));
            short MapY = (short)(rnd.Next(mapY - 1, mapY + 1));
            while (IsBlockedZone(MapX, MapY))
            {
                MapX = (short)(rnd.Next(mapX - 1, mapX + 1));
                MapY = (short)(rnd.Next(mapY - 1, mapY + 1));
            }

            ItemInstance newInstance = InventoryList.CreateItemInstance(drop.ItemVNum, 0);
            newInstance.Amount = drop.Amount;

            droppedItem = new MapItem(MapX, MapY, true)
            {
                ItemInstance = newInstance
            };

            while (ServerManager.GetMap(MapId).DroppedList.ContainsKey(random = rnd.Next(1, 999999)))
            { }
            droppedItem.ItemInstance.ItemInstanceId = random;
            ServerManager.GetMap(MapId).DroppedList.Add(random, droppedItem);

            Broadcast($"drop {droppedItem.ItemInstance.ItemVNum} {random} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.ItemInstance.Amount} 0 0 -1");
        }

        public List<MapMonster> GetListMonsterInRange(short mapX, short mapY, byte distance)
        {
            List<MapMonster> listmon = new List<MapMonster>();
            foreach (MapMonster mo in Monsters.Where(s => s.Alive))
            {
                if (GetDistance(new MapCell() { X = mapX, Y = mapY }, new MapCell() { X = mo.MapX, Y = mo.MapY }) <= distance + 1)
                    listmon.Add(mo);
            }
            return listmon;
        }

        public bool IsBlockedZone(int x, int y)
        {
            if (y >= 0 && x >= 0 && y < _grid.GetLength(0) && x < _grid.GetLength(1) && _grid[y, x] != 0)
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

        public void MonsterLifeManager()
        {
            var rnd = new Random();
            Task MonsterLifeTask = null;
            foreach (MapMonster monster in Monsters.OrderBy(i => rnd.Next()))
            {
                MonsterLifeTask = new Task(() => monster.MonsterLife());
                MonsterLifeTask.Start();
            }
        }

        public void NpcLifeManager()
        {
            var rnd = new Random();
            Task NpcLifeTask = null;
            foreach (MapNpc npc in Npcs.OrderBy(i => rnd.Next()))           
            {
                NpcLifeTask = new Task(() => npc.NpcLife());
                NpcLifeTask.Start();
            }
        }

        internal bool GetFreePosition(ref short firstX, ref short firstY, byte xpoint, byte ypoint)
        {
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            short MinX = (short)(-xpoint + firstX);
            short MaxX = (short)(xpoint + firstX);

            short MinY = (short)(-ypoint + firstY);
            short MaxY = (short)(ypoint + firstY);

            List<MapCell> cells = new List<MapCell>();
            for (short y = MinY; y <= MaxY; y++)
            {
                for (short x = MinX; x <= MaxX; x++)
                {
                    if (x != firstX && y != firstY)
                        cells.Add(new MapCell() { X = x, Y = y, MapId = MapId });
                }
            }

            foreach (MapCell cell in cells.OrderBy(s => r.Next(int.MaxValue)))
            {
                if (!ServerManager.GetMap(MapId).IsBlockedZone(firstX, firstY, cell.X, cell.Y))
                {
                    firstX = cell.X;
                    firstY = cell.Y;
                    return true;
                }
            }

            return false;
        }

        internal IEnumerable<Character> GetListPeopleInRange(short mapX, short mapY, byte distance)
        {
            List<Character> listch = new List<Character>();
            IEnumerable<ClientSession> cl = Sessions.Where(s => s.Character != null && s.Character.Hp > 0);
            for (int i = cl.Count() - 1; i >= 0; i--)
            {
                if (GetDistance(new MapCell() { X = mapX, Y = mapY }, new MapCell() { X = cl.ElementAt(i).Character.MapX, Y = cl.ElementAt(i).Character.MapY }) <= distance + 1)
                    listch.Add(cl.ElementAt(i).Character);
            }
            return listch;
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