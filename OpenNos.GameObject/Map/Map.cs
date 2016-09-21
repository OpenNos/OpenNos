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

using EpPathFinding.cs;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Map : BroadcastableBase, IMapDTO
    {
        #region Members

        private BaseGrid _grid;
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
            IEnumerable<PortalDTO> portals = DAOFactory.PortalDAO.LoadByMap(MapId);
            _portals = new List<Portal>();
            DroppedList = new ConcurrentDictionary<long, MapItem>();

            MapTypes = new List<MapType>();
            foreach (MapTypeMapDTO maptypemap in DAOFactory.MapTypeMapDAO.LoadByMapId(mapId))
            {
                MapTypeDTO MT = DAOFactory.MapTypeDAO.LoadById(maptypemap.MapTypeId);
                //Replace by MAPPING
                MapType maptype = new MapType()
                {
                    MapTypeId = MT.MapTypeId,
                    MapTypeName = MT.MapTypeName,
                    PotionDelay = MT.PotionDelay
                };
                ///////////////
                MapTypes.Add(maptype);
            }

            UserShops = new Dictionary<long, MapShop>();
            foreach (PortalDTO portal in portals)
            {//Replace by MAPPING
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
                //////////////////
            }

            _monsters = new List<MapMonster>();
            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(MapId).ToList())
            {
                _monsters.Add(new MapMonster(monster, this));
            }
            IEnumerable<MapNpcDTO> npcsDTO = DAOFactory.MapNpcDAO.LoadFromMap(MapId).ToList();

            _npcs = new List<MapNpc>();
            foreach (MapNpcDTO npc in npcsDTO)
            {
                _npcs.Add(new MapNpc(npc, this));
            }

            JumpPointParameters = new JumpPointParam(this._grid, new GridPos(0, 0), new GridPos(0, 0), false, true, true, HeuristicMode.MANHATTAN);
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        public ConcurrentDictionary<long, MapItem> DroppedList { get; set; }

        public int IsDancing { get; set; }

        public JumpPointParam JumpPointParameters { get; set; }
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

        public bool ShopAllowed { get; set; }

        public Dictionary<long, MapShop> UserShops { get; set; }

        public int XLength { get; set; }

        public int YLength { get; set; }

        #endregion

        #region Methods

        public static int GetDistance(Character character1, Character character2)
        {
            return GetDistance(new MapCell() { MapId = character1.MapId, X = character1.MapX, Y = character1.MapY }, new MapCell() { MapId = character2.MapId, X = character2.MapX, Y = character2.MapY });
        }

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
            try
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
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<MapCell>();
            }
        }

        public void DropItemByMonster(long? Owner, DropDTO drop, short mapX, short mapY)
        {
            try
            {
                Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                MapItem droppedItem = null;
                short localMapX = (short)(rnd.Next(mapX - 1, mapX + 1));
                short localMapY = (short)(rnd.Next(mapY - 1, mapY + 1));
                while (IsBlockedZone(localMapX, localMapY))
                {
                    localMapX = (short)(rnd.Next(mapX - 1, mapX + 1));
                    localMapY = (short)(rnd.Next(mapY - 1, mapY + 1));
                }

                ItemInstance newInstance = InventoryList.CreateItemInstance(drop.ItemVNum);
                newInstance.Amount = drop.Amount;

                droppedItem = new MapItem(localMapX, localMapY)
                {
                    ItemInstance = newInstance,
                    Owner = Owner
                };

                //rarify
                if (droppedItem.ItemInstance.Item.EquipmentSlot == (byte)EquipmentType.Armor || droppedItem.ItemInstance.Item.EquipmentSlot == (byte)EquipmentType.MainWeapon || droppedItem.ItemInstance.Item.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)
                    droppedItem.Rarify(null);

                DroppedList.TryAdd(droppedItem.ItemInstance.TransportId, droppedItem);

                Broadcast($"drop {droppedItem.ItemInstance.ItemVNum} {droppedItem.ItemInstance.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.ItemInstance.Amount} 0 0 -1");//TODO UseTransportId
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public IEnumerable<string> GenerateUserShops()
        {
            return UserShops.Select(shop => $"shop 1 {shop.Key + 1} 1 3 0 {shop.Value.Name}").ToList();
        }

        public List<MapMonster> GetListMonsterInRange(short mapX, short mapY, byte distance)
        {
            List<MapMonster> monsters = new List<MapMonster>();
            foreach (MapMonster monster in Monsters.Where(s => s.Alive))
            {
                if (GetDistance(new MapCell() { X = mapX, Y = mapY }, new MapCell() { X = monster.MapX, Y = monster.MapY }) <= distance + 1)
                    monsters.Add(monster);
            }
            return monsters;
        }

        public bool IsBlockedZone(int x, int y)
        {
            if (y >= 0 && x >= 0 && y < _grid.height && x < _grid.width && !_grid.IsWalkableAt(x, y))
            {
                return true;
            }

            return false;
        }

        public bool IsBlockedZone(int firstX, int firstY, int mapX, int mapY)
        {
            for (int i = 1; i <= Math.Abs(mapX - firstX); i++)
            {
                if (IsBlockedZone(firstX + Math.Sign(mapX - firstX) * i, firstY))
                {
                    return true;
                }
            }

            for (int i = 1; i <= Math.Abs(mapY - firstY); i++)
            {
                if (IsBlockedZone(firstX, firstY + Math.Sign(mapY - firstY) * i))
                {
                    return true;
                }
            }
            return false;
        }

        public List<MapCell> JPSPlus(MapCell cell1, MapCell cell2)
        {
            List<MapCell> path = new List<MapCell>();
            List<GridPos> lpath = new List<GridPos>();
            if (cell1.MapId != cell2.MapId)
            {
                return path;
            }

            JumpPointParameters.Reset(new GridPos(cell1.X, cell1.Y), new GridPos(cell2.X, cell2.Y));
            List<GridPos> resultPathList = JumpPointFinder.FindPath(JumpPointParameters);
            lpath = JumpPointFinder.GetFullPath(resultPathList);
            Debug.WriteLine($"From X: {cell1.X} Y: {cell1.Y}, To X: {cell2.X} Y: {cell2.Y}, Paths: {resultPathList.Count}, LPath: {lpath.Count}");
            if (lpath.Count > 0)
            {
                foreach (GridPos item in lpath)
                    path.Add(new MapCell { X = Convert.ToInt16(item.x), Y = Convert.ToInt16(item.y), MapId = cell1.MapId });
            }

            return path;
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

            _grid = new StaticGrid(XLength, YLength);
            for (int i = 0; i < YLength; ++i)
            {
                for (int t = 0; t < XLength; ++t)
                {
                    stream.Read(bytes, numBytesRead, numBytesToRead);
                    _grid.SetWalkableAt(t, i, (Convert.ToChar(bytes[0]) == 0 ? true : false));
                }
            }
        }

        public async void MonsterLifeManager()
        {
            try
            {
                var rnd = new Random();
                List<Task> MonsterLifeTask = new List<Task>();
                Monsters.RemoveAll(s => !s.Alive && !s.Respawn);
                foreach (MapMonster monster in Monsters.OrderBy(i => rnd.Next()))
                {
                    MonsterLifeTask.Add(new Task(() => monster.MonsterLife()));
                    MonsterLifeTask.Last().Start();
                }
                foreach (Task monsterLiveTask in MonsterLifeTask)
                    await monsterLiveTask;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public async void NpcLifeManager()
        {
            try
            {
                var rnd = new Random();
                List<Task> NpcLifeTask = new List<Task>();
                foreach (MapNpc npc in Npcs.OrderBy(i => rnd.Next()))
                {
                    NpcLifeTask.Add(new Task(() => npc.NpcLife()));
                    NpcLifeTask.Last().Start();
                }
                foreach (Task t in NpcLifeTask)
                    await t;
            }
            catch (Exception e)
            {
                Logger.Error(e);
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
                if (!IsBlockedZone(firstX, firstY, cell.X, cell.Y))
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
            List<Character> characters = new List<Character>();
            IEnumerable<ClientSession> cl = Sessions.Where(s => s.Character != null && s.Character.Hp > 0);
            for (int i = cl.Count() - 1; i >= 0; i--)
            {
                if (GetDistance(new MapCell() { X = mapX, Y = mapY }, new MapCell() { X = cl.ElementAt(i).Character.MapX, Y = cl.ElementAt(i).Character.MapY }) <= distance + 1)
                    characters.Add(cl.ElementAt(i).Character);
            }
            return characters;
        }

        internal async void MapTaskManager()
        {
            try
            {
                Task npcLifeTask = new Task(() => NpcLifeManager());
                npcLifeTask.Start();
                Task monsterLifeTask = new Task(() => MonsterLifeManager());
                monsterLifeTask.Start();
                Task characterLifeTask = new Task(() => CharacterLifeManager());
                characterLifeTask.Start();

                RemoveMapItem();

                await npcLifeTask;
                await monsterLifeTask;
                await characterLifeTask;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal List<MapCell> StraightPath(MapCell mapCell1, MapCell mapCell2)
        {
            List<MapCell> Path = new List<MapCell>();
            Path.Add(mapCell1);
            do
            {
                if (Path.Last().X < mapCell2.X && Path.Last().Y < mapCell2.Y)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X + 1), Y = (short)(Path.Last().Y + 1) });
                }
                else if (Path.Last().X > mapCell2.X && Path.Last().Y > mapCell2.Y)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X - 1), Y = (short)(Path.Last().Y - 1) });
                }
                else if (Path.Last().X < mapCell2.X && Path.Last().Y > mapCell2.Y)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X + 1), Y = (short)(Path.Last().Y - 1) });
                }
                else if (Path.Last().X > mapCell2.X && Path.Last().Y < mapCell2.Y)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X - 1), Y = (short)(Path.Last().Y + 1) });
                }
                else if (Path.Last().X > mapCell2.X)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X - 1), Y = (short)(Path.Last().Y) });
                }
                else if (Path.Last().X < mapCell2.X)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X + 1), Y = (short)(Path.Last().Y) });
                }
                else if (Path.Last().Y > mapCell2.Y)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X), Y = (short)(Path.Last().Y - 1) });
                }
                else if (Path.Last().Y < mapCell2.Y)
                {
                    Path.Add(new MapCell() { MapId = MapId, X = (short)(Path.Last().X), Y = (short)(Path.Last().Y + 1) });
                }
            }
            while ((Path.Last().X != mapCell2.X || Path.Last().Y != mapCell2.Y) && (!IsBlockedZone(Path.Last().X, Path.Last().Y)));
            if (IsBlockedZone(Path.Last().X, Path.Last().Y))
                Path.Remove(Path.Last());
            Path.RemoveAt(0);
            return Path;
        }

        private void CharacterLifeManager()
        {
            try
            {
                List<Task> NpcLifeTask = new List<Task>();
                foreach (ClientSession Session in Sessions.Where(s => s?.Character != null))
                {
                    int x = 1;
                    bool change = false;
                    if (Session.Character.Hp == 0 && Session.Character.LastHealth.AddSeconds(2) <= DateTime.Now)
                    {
                        Session.Character.Mp = 0;
                        Session.SendPacket(Session.Character.GenerateStat());
                        Session.Character.LastHealth = DateTime.Now;
                        continue;
                    }
                    WearableInstance amulet = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((short)EquipmentType.Amulet, InventoryType.Equipment);
                    if (Session.Character.LastEffect.AddSeconds(5) <= DateTime.Now && amulet != null)
                    {
                        if (amulet.ItemVNum == 4503 || amulet.ItemVNum == 4504)
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateEff(amulet.Item.EffectValue + (Session.Character.Class == (byte)ClassType.Adventurer ? 0 : Session.Character.Class - 1)), ReceiverType.All);
                        else
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateEff(amulet.Item.EffectValue), ReceiverType.All);
                        Session.Character.LastEffect = DateTime.Now;
                    }

                    if ((Session.Character.LastHealth.AddSeconds(2) <= DateTime.Now) || (Session.Character.IsSitting && Session.Character.LastHealth.AddSeconds(1.5) <= DateTime.Now))
                    {
                        Session.Character.LastHealth = DateTime.Now;
                        if (Session.healthStop == true)
                        {
                            Session.healthStop = false;
                            return;
                        }

                        if (Session.Character.LastDefence.AddSeconds(2) <= DateTime.Now && Session.Character.LastSkill.AddSeconds(2) <= DateTime.Now && Session.Character.Hp > 0)
                        {
                            if (x == 0)
                                x = 1;
                            if (Session.Character.Hp + Session.Character.HealthHPLoad() < Session.Character.HPLoad())
                            {
                                change = true;
                                Session.Character.Hp += Session.Character.HealthHPLoad();
                            }
                            else
                            {
                                if (Session.Character.Hp != (int)Session.Character.HPLoad())
                                    change = true;
                                Session.Character.Hp = (int)Session.Character.HPLoad();
                            }
                            if (x == 1)
                            {
                                if (Session.Character.Mp + Session.Character.HealthMPLoad() < Session.Character.MPLoad())
                                {
                                    Session.Character.Mp += Session.Character.HealthMPLoad();
                                    change = true;
                                }
                                else
                                {
                                    if (Session.Character.Mp != (int)Session.Character.MPLoad())
                                        change = true;
                                    Session.Character.Mp = (int)Session.Character.MPLoad();
                                }
                                x = 0;
                            }
                            if (change)
                            {
                                Session.SendPacket(Session.Character.GenerateStat());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void RemoveMapItem()
        {
            //take the data from list to remove it without having enumeration problems (ToList)
            try
            {
                IEnumerable<KeyValuePair<long, MapItem>> dropsToRemove = DroppedList.Where(dl => dl.Value.CreateDate.AddMinutes(3) < DateTime.Now).ToList();

                foreach (KeyValuePair<long, MapItem> drop in dropsToRemove)
                {
                    Broadcast(drop.Value.GenerateOut(drop.Key));
                    MapItem mapItem;
                    DroppedList.TryRemove(drop.Key, out mapItem);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
    }
}