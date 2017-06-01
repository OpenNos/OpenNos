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
using OpenNos.GameObject.Helpers;
using OpenNos.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class MapInstance : BroadcastableBase
    {
        #region Members

        private readonly List<int> _mapMonsterIds;

        private readonly List<int> _mapNpcIds;

        private readonly ThreadSafeSortedList<long, MapMonster> _monsters;

        private readonly ThreadSafeSortedList<long, MapNpc> _npcs;

        private readonly List<Portal> _portals;

        private readonly Random _random;

        private bool _disposed;

        private bool _isSleeping;

        private bool _isSleepingRequest;

        #endregion

        #region Instantiation

        public MapInstance(Map map, Guid guid, bool shopAllowed, MapInstanceType type, InstanceBag instanceBag)
        {
            Buttons = new List<MapButton>();
            XpRate = 1;
            DropRate = 1;
            ShopAllowed = shopAllowed;
            MapInstanceType = type;
            _isSleeping = true;
            LastUserShopId = 0;
            InstanceBag = instanceBag;
            Clock = new Clock(3);
            _random = new Random();
            Map = map;
            MapInstanceId = guid;
            ScriptedInstances = new List<ScriptedInstance>();
            OnCharacterDiscoveringMapEvents = new List<Tuple<EventContainer, List<long>>>();
            OnMoveOnMapEvents = new List<EventContainer>();
            OnMapClean = new List<EventContainer>();
            _monsters = new ThreadSafeSortedList<long, MapMonster>();
            _npcs = new ThreadSafeSortedList<long, MapNpc>();
            _mapMonsterIds = new List<int>();
            _mapNpcIds = new List<int>();
            DroppedList = new ThreadSafeSortedList<long, MapItem>();
            _portals = new List<Portal>();
            UserShops = new Dictionary<long, MapShop>();
            StartLife();
        }

        #endregion

        #region Properties

        public List<MapButton> Buttons { get; set; }

        public Clock Clock { get; set; }

        public ThreadSafeSortedList<long, MapItem> DroppedList { get; }

        public int DropRate { get; set; }

        public InstanceBag InstanceBag { get; set; }

        public bool IsDancing { get; set; }

        public bool IsPVP { get; set; }

        public bool IsSleeping
        {
            get
            {
                if (_isSleepingRequest && !_isSleeping && LastUnregister.AddSeconds(30) < DateTime.Now)
                {
                    _isSleeping = true;
                    _isSleepingRequest = false;
                    return true;
                }
                return _isSleeping;
            }
            set
            {
                if (value)
                {
                    _isSleepingRequest = true;
                }
                else
                {
                    _isSleeping = false;
                    _isSleepingRequest = false;
                }
            }
        }

        public long LastUserShopId { get; set; }

        public Map Map { get; set; }

        public byte MapIndexX { get; set; }

        public byte MapIndexY { get; set; }

        public Guid MapInstanceId { get; set; }

        public MapInstanceType MapInstanceType { get; set; }

        public List<MapMonster> Monsters => _monsters.GetAllItems();

        public List<MapNpc> Npcs => _npcs.GetAllItems();

        public List<Tuple<EventContainer, List<long>>> OnCharacterDiscoveringMapEvents { get; set; }

        public List<EventContainer> OnMapClean { get; set; }

        public List<EventContainer> OnMoveOnMapEvents { get; set; }

        public List<Portal> Portals => _portals;

        public bool ShopAllowed { get; set; }

        public List<ScriptedInstance> ScriptedInstances { get; set; }

        public Dictionary<long, MapShop> UserShops { get; }

        public int XpRate { get; set; }

        #endregion

        #region Methods

        public void AddMonster(MapMonster monster)
        {
            _monsters[monster.MapMonsterId] = monster;
        }

        public void AddNPC(MapNpc monster)
        {
            _npcs[monster.MapNpcId] = monster;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                foreach (ClientSession session in ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.MapInstanceId == MapInstanceId))
                {
                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId, session.Character.MapX, session.Character.MapY);
                }
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public void DropItemByMonster(long? owner, DropDTO drop, short mapX, short mapY)
        {
            try
            {
                short localMapX = mapX;
                short localMapY = mapY;
                List<MapCell> possibilities = new List<MapCell>();

                for (short x = -1; x < 2; x++)
                {
                    for (short y = -1; y < 2; y++)
                    {
                        possibilities.Add(new MapCell { X = x, Y = y });
                    }
                }

                foreach (MapCell possibilitie in possibilities.OrderBy(s => ServerManager.Instance.RandomNumber()))
                {
                    localMapX = (short)(mapX + possibilitie.X);
                    localMapY = (short)(mapY + possibilitie.Y);
                    if (!Map.IsBlockedZone(localMapX, localMapY))
                    {
                        break;
                    }
                }

                MonsterMapItem droppedItem = new MonsterMapItem(localMapX, localMapY, drop.ItemVNum, drop.Amount, owner ?? -1);

                DroppedList[droppedItem.TransportId] = droppedItem;

                Broadcast($"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} 0 0 -1");
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void DropItems(List<Tuple<short, int, short, short>> list)
        {
            foreach (Tuple<short, int, short, short> drop in list)
            {
                MonsterMapItem droppedItem = new MonsterMapItem(drop.Item3, drop.Item4, drop.Item1, drop.Item2);

                DroppedList[droppedItem.TransportId] = droppedItem;

                Broadcast($"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} 0 0 -1");
            }
        }

        public IEnumerable<string> GenerateNPCShopOnMap()
        {
            return (from npc in Npcs where npc.Shop != null select $"shop 2 {npc.MapNpcId} {npc.Shop.ShopId} {npc.Shop.MenuType} {npc.Shop.ShopType} {npc.Shop.Name}").ToList();
        }

        public IEnumerable<string> GeneratePlayerShopOnMap()
        {
            return UserShops.Select(shop => $"pflag 1 {shop.Value.OwnerId} {shop.Key + 1}").ToList();
        }

        public string GenerateRsfn(bool isInit = false)
        {
            if (MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                return $"rsfn {MapIndexX} {MapIndexY} {(isInit ? 1 : (Monsters.Where(s => s.IsAlive).ToList().Count == 0 ? 0 : 1))}";
            }
            return string.Empty;
        }

        public IEnumerable<string> GenerateUserShops()
        {
            return UserShops.Select(shop => $"shop 1 {shop.Value.OwnerId} 1 3 0 {shop.Value.Name}").ToList();
        }

        public List<MapMonster> GetListMonsterInRange(short mapX, short mapY, byte distance)
        {
            return _monsters.GetAllItems().Where(s => s.IsAlive && s.IsInRange(mapX, mapY, distance)).ToList();
        }

        public List<string> GetMapItems()
        {
            List<string> packets = new List<string>();
            Sessions.Where(s => s.Character != null && !s.Character.InvisibleGm).ToList().ForEach(s =>
            {
                s.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m => packets.Add(m.GenerateIn()));
            });

            Portals.ForEach(s => packets.Add(s.GenerateGp()));
            ScriptedInstances.Where(s => s.Type == ScriptedInstanceType.TimeSpace).ToList().ForEach(s => packets.Add(s.GenerateWp()));
           
            Monsters.ForEach(s => packets.Add(s.GenerateIn()));
            Npcs.ForEach(s => packets.Add(s.GenerateIn()));
            packets.AddRange(GenerateNPCShopOnMap());
            DroppedList.GetAllItems().ForEach(s => packets.Add(s.GenerateIn()));

            Buttons.ForEach(s => packets.Add(s.GenerateIn()));
            packets.AddRange(GenerateUserShops());
            packets.AddRange(GeneratePlayerShopOnMap());
            return packets;
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

        public int GetNextNpcId()
        {
            int nextId = _mapNpcIds.Any() ? _mapNpcIds.Last() + 1 : 1;
            _mapNpcIds.Add(nextId);
            return nextId;
        }

        public void LoadMonsters()
        {
            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(Map.MapId).ToList())
            {
                MapMonster mo = monster as MapMonster;
                mo.Initialize(this);
                _monsters[mo.MapMonsterId] = mo;
                _mapMonsterIds.Add(mo.MapMonsterId);
            }
        }

        public void LoadNpcs()
        {
            foreach (MapNpcDTO npc in DAOFactory.MapNpcDAO.LoadFromMap(Map.MapId).ToList())
            {
                MapNpc np = npc as MapNpc;
                np.Initialize(this);
                _npcs[np.MapNpcId] = np;
                _mapNpcIds.Add(np.MapNpcId);
            }
        }

        public void LoadPortals()
        {
            foreach (PortalDTO portal in DAOFactory.PortalDAO.LoadByMap(Map.MapId).ToList())
            {
                Portal portal2 = (Portal)portal;
                portal2.SourceMapInstanceId = MapInstanceId;
                _portals.Add(portal2);
            }
        }

        public void MapClear()
        {
            Broadcast("mapclear");
            GetMapItems().ForEach(s => Broadcast(s));
        }

        public MapItem PutItem(InventoryType type, short slot, byte amount, ref ItemInstance inv, ClientSession session)
        {
            Logger.Debug(session.GenerateIdentity(), $"type: {type} slot: {slot} amount: {amount}");
            Guid random2 = Guid.NewGuid();
            MapItem droppedItem = null;
            List<GridPos> possibilities = new List<GridPos>();

            for (short x = -2; x < 3; x++)
            {
                for (short y = -2; y < 3; y++)
                {
                    possibilities.Add(new GridPos { X = x, Y = y });
                }
            }

            short mapX = 0;
            short mapY = 0;
            bool niceSpot = false;
            foreach (GridPos possibilitie in possibilities.OrderBy(s => _random.Next()))
            {
                mapX = (short)(session.Character.PositionX + possibilitie.X);
                mapY = (short)(session.Character.PositionY + possibilitie.Y);
                if (!Map.IsBlockedZone(mapX, mapY))
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

        public void RemoveMapItem()
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

        public void RemoveMonster(MapMonster monsterToRemove)
        {
            _monsters.Remove(monsterToRemove.MapMonsterId);
        }

        public void SpawnButton(MapButton parameter)
        {
            Buttons.Add(parameter);
            Broadcast(parameter.GenerateIn());
        }

        public void UnspawnMonsters(int monsterVnum)
        {
            _monsters.GetAllItems().Where(s => s.MonsterVNum == monsterVnum).ToList().ForEach(s =>
             {
                 s.IsAlive = false;
                 s.LastMove = DateTime.Now;
                 s.CurrentHp = 0;
                 s.CurrentMp = 0;
                 s.Death = DateTime.Now;
                 Broadcast(s.GenerateOut());
             });
        }

        internal void CreatePortal(Portal portal)
        {
            portal.SourceMapInstanceId = MapInstanceId;
            _portals.Add(portal);
            Broadcast(portal.GenerateGp());
        }

        internal IEnumerable<Character> GetCharactersInRange(short mapX, short mapY, byte distance)
        {
            List<Character> characters = new List<Character>();
            IEnumerable<ClientSession> cl = Sessions.Where(s => s.HasSelectedCharacter && s.Character.Hp > 0);
            IEnumerable<ClientSession> clientSessions = cl as IList<ClientSession> ?? cl.ToList();
            for (int i = clientSessions.Count() - 1; i >= 0; i--)
            {
                if (Map.GetDistance(new MapCell { X = mapX, Y = mapY }, new MapCell { X = clientSessions.ElementAt(i).Character.PositionX, Y = clientSessions.ElementAt(i).Character.PositionY }) <= distance + 1)
                {
                    characters.Add(clientSessions.ElementAt(i).Character);
                }
            }
            return characters;
        }

        internal void RemoveMonstersTarget(long characterId)
        {
            foreach (MapMonster monster in Monsters.Where(m => m.Target == characterId))
            {
                monster.RemoveTarget();
            }
        }

        internal void StartLife()
        {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x =>
            {
                try
                {
                    if (Monsters.Count(s => s.IsAlive) == 0)
                    {
                        OnMapClean.ForEach(e =>
                        {
                            EventHelper.Instance.RunEvent(e);
                        });
                        OnMapClean.RemoveAll(s => s != null);
                    }
                    if (!IsSleeping)
                    {
                        RemoveMapItem();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
        }

        internal List<int> SummonMonsters(List<MonsterToSummon> summonParameters)
        {
            List<int> ids = new List<int>();
            foreach (MonsterToSummon mon in summonParameters)
            {
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(mon.VNum);
                if (npcmonster != null)
                {
                    MapMonster monster = new MapMonster { MonsterVNum = npcmonster.NpcMonsterVNum, MapY = mon.SpawnCell.Y, MapX = mon.SpawnCell.X, MapId = Map.MapId, IsMoving = mon.IsMoving, MapMonsterId = GetNextMonsterId(), ShouldRespawn = false, Target = mon.Target, OnDeathEvents = mon.DeathEvents, IsTarget = mon.IsTarget, IsBonus = mon.IsBonus };
                    monster.Initialize(this);
                    monster.IsHostile = mon.IsHostile;
                    monster.Monster.BCards.ForEach(c => c.ApplyBCards(monster));
                    AddMonster(monster);
                    Broadcast(monster.GenerateIn());
                    ids.Add(monster.MapMonsterId);
                }
            }

            return ids;
        }

        internal List<int> SummonNpcs(List<NpcToSummon> summonParameters)
        {
            List<int> ids = new List<int>();
            foreach (NpcToSummon mon in summonParameters)
            {
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(mon.VNum);
                if (npcmonster != null)
                {
                    MapNpc npc = new MapNpc { NpcVNum = npcmonster.NpcMonsterVNum, MapY = mon.SpawnCell.X, MapX = mon.SpawnCell.Y, MapId = Map.MapId, IsHostile = true, IsMoving = true, MapNpcId = GetNextNpcId(), Target = mon.Target, OnDeathEvents = mon.DeathEvents, IsMate = mon.IsMate, IsProtected = mon.IsProtected };
                    npc.Initialize(this);
                    AddNPC(npc);
                    Broadcast(npc.GenerateIn());
                    ids.Add(npc.MapNpcId);
                }
            }

            return ids;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monsters.Dispose();
                _npcs.Dispose();
            }
        }

        #endregion
    }
}