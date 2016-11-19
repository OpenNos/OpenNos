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

using EpPathFinding;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
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
        private bool _disposed;
        private StaticGrid _grid;
        private List<int> _mapMonsterIds;
        private List<MapNpc> _npcs;
        private List<PortalDTO> _portals;
        private Random _random;
        private Guid _uniqueIdentifier;

        #endregion

        #region Instantiation

        public Map(short mapId, Guid uniqueIdentifier, byte[] data)
        {
            _random = new Random();
            MapId = mapId;
            ShopAllowed = true;
            _uniqueIdentifier = uniqueIdentifier;
            _monsters = new ThreadSafeSortedList<long, MapMonster>();
            _mapMonsterIds = new List<int>();
            Data = data;
            LoadZone();
            IEnumerable<PortalDTO> portals = DAOFactory.PortalDAO.LoadByMap(MapId).ToList();
            DroppedList = new ThreadSafeSortedList<long, MapItem>();

            MapTypes = new List<MapTypeDTO>();
            foreach (MapTypeMapDTO maptypemap in DAOFactory.MapTypeMapDAO.LoadByMapId(mapId).ToList())
            {
                MapTypeDTO maptype = DAOFactory.MapTypeDAO.LoadById(maptypemap.MapTypeId) as MapTypeDTO;
                MapTypes.Add(maptype);
            }

            _portals = new List<PortalDTO>();
            foreach (PortalDTO portal in portals)
            {
                _portals.Add(portal as PortalDTO);
            }

            UserShops = new Dictionary<long, MapShop>();
            _npcs = new List<MapNpc>();
            _npcs.AddRange(ServerManager.Instance.GetMapNpcsByMapId(MapId).AsEnumerable<MapNpc>());
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        public bool Disabled { get; internal set; }

        public ThreadSafeSortedList<long, MapItem> DroppedList { get; set; }

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

        public override void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public void DropItemByMonster(long? Owner, DropDTO drop, short mapX, short mapY, int gold = 0)
        {
            try
            {
                MonsterMapItem droppedItem = null;
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

                droppedItem = new MonsterMapItem(localMapX, localMapY, drop.ItemVNum, drop.Amount, Owner ?? -1);

                DroppedList[droppedItem.TransportId] = droppedItem;

                Broadcast($"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} 0 0 -1");
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal void RemoveMonstersTarget(long characterId)
        {
            foreach (MapMonster monster in Monsters.Where(m => m.Target == characterId))
            {
                monster.RemoveTarget();
            }
        }

        public IEnumerable<string> GenerateUserShops()
        {
            return UserShops.Select(shop => $"shop 1 {shop.Value.OwnerId} 1 3 0 {shop.Value.Name}").ToList();
        }

        public List<MapMonster> GetListMonsterInRange(short mapX, short mapY, byte distance)
        {
            return _monsters.GetAllItems().Where(s => s.IsAlive && s.IsInRange(mapX, mapY, distance)).ToList();
        }

        public MapMonster GetMonster(long mapMonsterId)
        {
            return _monsters[mapMonsterId];
        }

        public int GetNextMonsterId()
        {
            int nextId = _mapMonsterIds.Any() ? _mapMonsterIds.Last() + 1 : 1;
            _mapMonsterIds.Add(nextId);
            return nextId;
        }

        public bool IsBlockedZone(int x, int y)
        {
            if (!_grid.IsWalkableAt(new GridPos(x, y)))
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

        public List<GridPos> JPSPlus(GridPos cell1, GridPos cell2)
        {
            List<GridPos> lpath = new List<GridPos>();

            JumpPointParameters.Reset(cell1, cell2);
            List<GridPos> resultPathList = JumpPointFinder.FindPath(JumpPointParameters);
            lpath = JumpPointFinder.GetFullPath(resultPathList);
            Debug.WriteLine($"From X: {cell1.x} Y: {cell1.y}, To X: {cell2.x} Y: {cell2.y}, Paths: {resultPathList.Count}, LPath: {lpath.Count}");
            return lpath;
        }

        public void LoadMonsters()
        {
            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(MapId).ToList())
            {
                _monsters[monster.MapMonsterId] = monster as MapMonster;
                _mapMonsterIds.Add(monster.MapMonsterId);
            }
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
                    _grid.SetWalkableAt(new GridPos(t, i), bytes[0]);
                }
            }

            // initialize JPS _tempgrid = ConvertToGrid(_grid);
            JumpPointParameters = new JumpPointParam(_grid, new GridPos(0, 0), new GridPos(0, 0), false, true, true, HeuristicMode.MANHATTAN);
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

        public MapItem PutItem(InventoryType type, short slot, byte amount, ref ItemInstance inv, ClientSession session)
        {
            Logger.Debug($"type: {type} slot: {slot} amount: {amount}", session.SessionId);
            Guid random2 = Guid.NewGuid();
            MapItem droppedItem = null;
            List<GridPos> Possibilities = new List<GridPos>();

            for (short x = -2; x < 3; x++)
            {
                for (short y = -2; y < 3; y++)
                {
                    Possibilities.Add(new GridPos() { x = x, y = y });
                }
            }

            short mapX = 0;
            short mapY = 0;
            bool niceSpot = false;
            foreach (GridPos possibilitie in Possibilities.OrderBy(s => _random.Next()))
            {
                mapX = (short)(session.Character.MapX + possibilitie.x);
                mapY = (short)(session.Character.MapY + possibilitie.y);
                if (!IsBlockedZone(mapX, mapY))
                {
                    niceSpot = true;
                    break;
                }
            }

            if (niceSpot)
            {
                if (amount > 0 && amount <= inv.Amount)
                {
                    ItemInstance newItemInstance = inv.DeepCopy();
                    newItemInstance.Id = random2;
                    newItemInstance.Amount = amount;
                    droppedItem = new CharacterMapItem(mapX, mapY, newItemInstance);

                    DroppedList[droppedItem.TransportId] = droppedItem;
                    inv.Amount -= amount;
                }
            }
            return droppedItem;
        }

        public void RemoveMonster(MapMonster monsterToRemove)
        {
            _monsters.Remove(monsterToRemove.MapMonsterId);
        }

        public void SetMapMapMonsterReference()
        {
            foreach (MapMonster monster in _monsters.GetAllItems())
            {
                monster.Map = this;
            }
        }

        public void SetMapMapNpcReference()
        {
            foreach (MapNpc npc in _npcs)
            {
                npc.Map = this;
            }
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

        internal List<GridPos> StraightPath(GridPos mapCell1, GridPos mapCell2)
        {
            List<GridPos> Path = new List<GridPos>();
            Path.Add(mapCell1);
            do
            {
                if (Path.Last().x < mapCell2.x && Path.Last().y < mapCell2.y)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x + 1), y = (short)(Path.Last().y + 1) });
                }
                else if (Path.Last().x > mapCell2.x && Path.Last().y > mapCell2.y)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x - 1), y = (short)(Path.Last().y - 1) });
                }
                else if (Path.Last().x < mapCell2.x && Path.Last().y > mapCell2.y)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x + 1), y = (short)(Path.Last().y - 1) });
                }
                else if (Path.Last().x > mapCell2.x && Path.Last().y < mapCell2.y)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x - 1), y = (short)(Path.Last().y + 1) });
                }
                else if (Path.Last().x > mapCell2.x)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x - 1), y = (short)(Path.Last().y) });
                }
                else if (Path.Last().x < mapCell2.x)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x + 1), y = (short)(Path.Last().y) });
                }
                else if (Path.Last().y > mapCell2.y)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x), y = (short)(Path.Last().y - 1) });
                }
                else if (Path.Last().y < mapCell2.y)
                {
                    Path.Add(new GridPos() { x = (short)(Path.Last().x), y = (short)(Path.Last().y + 1) });
                }
            }
            while ((Path.Last().x != mapCell2.x || Path.Last().y != mapCell2.y) && (!IsBlockedZone(Path.Last().x, Path.Last().y)));
            if (IsBlockedZone(Path.Last().x, Path.Last().y))
            {
                if (Path.Any())
                {
                    Path.Remove(Path.Last());
                }
            }
            Path.RemoveAt(0);
            return Path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monsters.Dispose();
            }
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
                        WearableInstance amulet = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Amulet, InventoryType.Wear);
                        if (Session.Character.LastEffect.AddSeconds(5) <= DateTime.Now && amulet != null)
                        {
                            if (amulet.ItemVNum == 4503 || amulet.ItemVNum == 4504)
                            {
                                Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(amulet.Item.EffectValue + (Session.Character.Class == ClassType.Adventurer ? 0 : (byte)Session.Character.Class - 1)), Session.Character.MapX, Session.Character.MapY);
                            }
                            else
                            {
                                Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(amulet.Item.EffectValue), Session.Character.MapX, Session.Character.MapY);
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

                            if (Session.Character.LastDefence.AddSeconds(2) <= DateTime.Now && Session.Character.LastSkillUse.AddSeconds(2) <= DateTime.Now && Session.Character.Hp > 0)
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
            foreach (MapMonster monster in _monsters.GetAllItems().Where(s => !s.IsAlive && !s.Respawn.Value))
            {
                RemoveMonster(monster);
            }
        }

        private void RemoveMapItem()
        {
            // take the data from list to remove it without having enumeration problems (ToList)
            try
            {
                List<MapItem> dropsToRemove = DroppedList.GetAllItems().Where(dl => dl.CreatedDate.AddMinutes(3) < DateTime.Now).ToList();

                foreach (MapItem drop in dropsToRemove)
                {
                    Broadcast(drop.GenerateOut(drop.TransportId));
                    DroppedList.Remove(drop.TransportId);
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