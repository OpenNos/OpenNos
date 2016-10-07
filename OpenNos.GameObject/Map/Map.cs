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
using OpenNos.Core.Collections;
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

        private readonly ThreadSafeSortedList<long, MapMonster> _monsters;
        private short[,] _grid;
        private List<int> _mapMonsterIds;
        private List<MapNpc> _npcs;
        private List<PortalDTO> _portals;
        private Random _random;
        private BaseGrid _tempgrid;
        private Guid _uniqueIdentifier;

        #endregion

        #region Instantiation

        public Map(short mapId, Guid uniqueIdentifier, byte[] data)
        {
            _random = new Random();
            MapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            _monsters = new ThreadSafeSortedList<long, MapMonster>();
            _mapMonsterIds = new List<int>();
            Data = data;
            LoadZone();
            IEnumerable<PortalDTO> portals = DAOFactory.PortalDAO.LoadByMap(MapId).ToList();
            _portals = new List<PortalDTO>();
            DroppedList = new ConcurrentDictionary<long, MapItem>();

            MapTypes = new List<MapTypeDTO>();
            foreach (MapTypeMapDTO maptypemap in DAOFactory.MapTypeMapDAO.LoadByMapId(mapId).ToList())
            {
                MapTypeDTO MT = DAOFactory.MapTypeDAO.LoadById(maptypemap.MapTypeId);

                // Replace by MAPPING
                MapTypeDTO maptype = new MapTypeDTO()
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
            {
                // Replace by MAPPING
                _portals.Add(new PortalDTO()
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

            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(MapId).ToList())
            {
                _monsters[monster.MapMonsterId] = new MapMonster(monster, this);
                _mapMonsterIds.Add(monster.MapMonsterId);
            }
            IEnumerable<MapNpcDTO> npcsDTO = DAOFactory.MapNpcDAO.LoadFromMap(MapId).ToList();

            _npcs = new List<MapNpc>();
            foreach (MapNpcDTO npc in npcsDTO)
            {
                _npcs.Add(new MapNpc(npc, this));
            }
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        public bool Disabled { get; internal set; }

        public ConcurrentDictionary<long, MapItem> DroppedList { get; set; }

        public bool IsDancing { get; set; }

        public JumpPointParam JumpPointParameters { get; set; }

        public short MapId { get; set; }

        public List<MapTypeDTO> MapTypes
        {
            get; set;
        }

        /// <summary>
        /// This list ONLY for READ access to MapMonster, you CANNOT MODIFY them here. Use
        /// Add/RemoveMonster instead.
        /// </summary>
        public List<MapMonster> Monsters
        {
            get
            {
                return _monsters.GetAllItems();
            }
        }

        public int Music { get; set; }

        public string Name { get; set; }

        public List<MapNpc> Npcs
        {
            get
            {
                return _npcs;
            }
        }

        public List<PortalDTO> Portals
        {
            get
            {
                return _portals;
            }
        }

        public bool ShopAllowed { get; set; }

        public BaseGrid Tempgrid
        {
            get
            {
                if (_tempgrid == null)
                {
                    _tempgrid = ConvertToGrid(_grid);
                    JumpPointParameters = new JumpPointParam(_tempgrid, new GridPos(0, 0), new GridPos(0, 0), false, true, true, HeuristicMode.MANHATTAN);
                }

                return _tempgrid;
            }
        }

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
            return Math.Max(Math.Abs(p.X - q.X), Math.Abs(p.Y - q.Y));
        }

        public void AddMonster(MapMonster monster)
        {
            _monsters[monster.MapMonsterId] = monster;
        }

        public BaseGrid ConvertToGrid(short[,] _grid)
        {
            BaseGrid grid = new StaticGrid(XLength, YLength);
            for (int y = 0; y < YLength; ++y)
            {
                for (int x = 0; x < XLength; ++x)
                {
                    grid.SetWalkableAt(x, y, !IsBlockedZone(x, y));
                }
            }
            return grid;
        }

        public void DropItemByMonster(long? Owner, DropDTO drop, short mapX, short mapY)
        {
            try
            {
                MapItem droppedItem = null;
                short localMapX = (short)(_random.Next(mapX - 1, mapX + 1));
                short localMapY = (short)(_random.Next(mapY - 1, mapY + 1));
                List<MapCell> Possibilities = new List<MapCell>();

                for (short x = -1; x < 2; x++)
                {
                    for (short y = -1; y < 2; y++)
                    {
                        Possibilities.Add(new MapCell() { X = x, Y = y });
                    }
                }

                foreach (MapCell possibilitie in Possibilities.OrderBy(s => _random.Next()))
                {
                    localMapX = (short)(mapX + possibilitie.X);
                    localMapY = (short)(mapY + possibilitie.Y);
                    if (!IsBlockedZone(localMapX, localMapY))
                    {
                        break;
                    }
                }

                ItemInstance newInstance = InventoryList.CreateItemInstance(drop.ItemVNum);
                newInstance.Amount = drop.Amount;

                droppedItem = new MapItem(localMapX, localMapY)
                {
                    ItemInstance = newInstance,
                    Owner = Owner
                };

                // rarify
                if (droppedItem.ItemInstance.Item.EquipmentSlot == (byte)EquipmentType.Armor || droppedItem.ItemInstance.Item.EquipmentSlot == (byte)EquipmentType.MainWeapon || droppedItem.ItemInstance.Item.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)
                {
                    droppedItem.Rarify(null);
                }

                DroppedList.TryAdd(droppedItem.ItemInstance.TransportId, droppedItem);

                // TODO: UseTransportId
                Broadcast($"drop {droppedItem.ItemInstance.ItemVNum} {droppedItem.ItemInstance.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.ItemInstance.Amount} 0 0 -1");
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
            return _monsters.GetAllItems().Where(s => s.Alive && GetDistance(
                new MapCell()
                {
                    X = mapX,
                    Y = mapY
                }, new MapCell()
                {
                    X = s.MapX,
                    Y = s.MapY
                }) <= distance + 1).ToList();
        }

        public MapMonster GetMonster(long mapMonsterId)
        {
            return _monsters[mapMonsterId];
        }

        public int GetNextMonsterId()
        {
            int nextId = _mapMonsterIds.Max() + 1;
            _mapMonsterIds.Add(nextId);
            return nextId;
        }

        public bool IsBlockedZone(int x, int y)
        {
            if (y >= 0 && x >= 0 && y < _grid.GetLength(1) && x < _grid.GetLength(0) && _grid[x, y] != 0)
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
                {
                    path.Add(new MapCell { X = Convert.ToInt16(item.x), Y = Convert.ToInt16(item.y), MapId = cell1.MapId });
                }
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

            _grid = new short[XLength, YLength];
            for (int i = 0; i < YLength; ++i)
            {
                for (int t = 0; t < XLength; ++t)
                {
                    stream.Read(bytes, numBytesRead, numBytesToRead);
                    _grid[t, i] = Convert.ToInt16(bytes[0]);
                }
            }
        }

        public void MonsterLifeManager()
        {
            try
            {
                List<Task> MonsterLifeTask = new List<Task>();
                RemoveDeadMonsters();
                foreach (MapMonster monster in Monsters.OrderBy(i => _random.Next()))
                {
                    monster.MonsterLife();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void NpcLifeManager()
        {
            try
            {
                foreach (MapNpc npc in Npcs.OrderBy(i => _random.Next()))
                {
                    npc.NpcLife();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void RemoveMonster(MapMonster monsterToRemove)
        {
            _monsters.Remove(monsterToRemove.MapMonsterId);
        }

        internal bool GetFreePosition(ref short firstX, ref short firstY, byte xpoint, byte ypoint)
        {
            short MinX = (short)(-xpoint + firstX);
            short MaxX = (short)(xpoint + firstX);

            short MinY = (short)(-ypoint + firstY);
            short MaxY = (short)(ypoint + firstY);

            List<MapCell> cells = new List<MapCell>();
            for (short y = MinY; y <= MaxY; y++)
            {
                for (short x = MinX; x <= MaxX; x++)
                {
                    if (x != firstX || y != firstY)
                    {
                        cells.Add(new MapCell() { X = x, Y = y, MapId = MapId });
                    }
                }
            }

            foreach (MapCell cell in cells.OrderBy(s => _random.Next(int.MaxValue)))
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
            IEnumerable<ClientSession> cl = Sessions.Where(s => s.HasSelectedCharacter && s.Character.Hp > 0);
            for (int i = cl.Count() - 1; i >= 0; i--)
            {
                if (GetDistance(new MapCell() { X = mapX, Y = mapY }, new MapCell() { X = cl.ElementAt(i).Character.MapX, Y = cl.ElementAt(i).Character.MapY }) <= distance + 1)
                {
                    characters.Add(cl.ElementAt(i).Character);
                }
            }
            return characters;
        }

        internal void MapTaskManager()
        {
            try
            {
                List<Task> MapTasks = new List<Task>();
                MapTasks.Add(new Task(() => NpcLifeManager()));
                MapTasks.Add(new Task(() => MonsterLifeManager()));
                MapTasks.Add(new Task(() => CharacterLifeManager()));
                MapTasks.Add(new Task(() => RemoveMapItem()));

                MapTasks.ForEach(s => s.Start());
                Task.WaitAll(MapTasks.ToArray());
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
            {
                if (Path.Count > 0)
                {
                    Path.Remove(Path.Last());
                }
            }
            Path.RemoveAt(0);
            return Path;
        }

        private void CharacterLifeManager()
        {
            try
            {
                List<Task> NpcLifeTask = new List<Task>();
                for (int i = Sessions.Where(s => s.HasSelectedCharacter).Count() - 1; i >= 0; i--)
                {
                    ClientSession Session = Sessions.Where(s => s?.Character != null).ElementAt(i);
                    if (Session.Character.LastMailRefresh.AddSeconds(30) < DateTime.Now)
                    {
                        Session.Character.RefreshMail();
                    }
                    int x = 1;
                    bool change = false;
                    if (Session.Character.Hp == 0 && Session.Character.LastHealth.AddSeconds(2) <= DateTime.Now)
                    {
                        Session.Character.Mp = 0;
                        Session.SendPacket(Session.Character.GenerateStat());
                        Session.Character.LastHealth = DateTime.Now;
                    }
                    else
                    {
                        WearableInstance amulet = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((short)EquipmentType.Amulet, InventoryType.Equipment);
                        if (Session.Character.LastEffect.AddSeconds(5) <= DateTime.Now && amulet != null)
                        {
                            if (amulet.ItemVNum == 4503 || amulet.ItemVNum == 4504)
                            {
                                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateEff(amulet.Item.EffectValue + (Session.Character.Class == (byte)ClassType.Adventurer ? 0 : Session.Character.Class - 1)), ReceiverType.All);
                            }
                            else
                            {
                                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateEff(amulet.Item.EffectValue), ReceiverType.All);
                            }
                            Session.Character.LastEffect = DateTime.Now;
                        }

                        if ((Session.Character.LastHealth.AddSeconds(2) <= DateTime.Now) || (Session.Character.IsSitting && Session.Character.LastHealth.AddSeconds(1.5) <= DateTime.Now))
                        {
                            Session.Character.LastHealth = DateTime.Now;
                            if (Session.HealthStop)
                            {
                                Session.HealthStop = false;
                                return;
                            }

                            if (Session.Character.LastDefence.AddSeconds(2) <= DateTime.Now && Session.Character.LastSkill.AddSeconds(2) <= DateTime.Now && Session.Character.Hp > 0)
                            {
                                if (x == 0)
                                {
                                    x = 1;
                                }
                                if (Session.Character.Hp + Session.Character.HealthHPLoad() < Session.Character.HPLoad())
                                {
                                    change = true;
                                    Session.Character.Hp += Session.Character.HealthHPLoad();
                                }
                                else
                                {
                                    if (Session.Character.Hp != (int)Session.Character.HPLoad())
                                    {
                                        change = true;
                                    }
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
                                        {
                                            change = true;
                                        }
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
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void RemoveDeadMonsters()
        {
            foreach (MapMonster monster in _monsters.GetAllItems().Where(s => !s.Alive && !s.Respawn))
            {
                RemoveMonster(monster);
            }
        }

        private void RemoveMapItem()
        {
            // take the data from list to remove it without having enumeration problems (ToList)
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