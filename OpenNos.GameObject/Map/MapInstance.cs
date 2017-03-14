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
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class MapInstance : BroadcastableBase
    {
        #region Members

        private readonly List<int> _mapMonsterIds;

        private readonly ThreadSafeSortedList<long, MapMonster> _monsters;

        private readonly List<MapNpc> _npcs;

        private readonly List<Portal> _portals;

        private readonly Random _random;

        private bool _disposed;

        private bool _isSleeping;

        private bool _isSleepingRequest;

        #endregion

        #region Instantiation

        public MapInstance(Map map, Guid guid, bool shopAllowed, MapInstanceType type)
        {
            XpRate = 1;
            DropRate = 1;
            NpcEffectActivated = true;
            ShopAllowed = shopAllowed;
            MapInstanceType = type;
            _isSleeping = true;
            LastUserShopId = 0;
            _random = new Random();
            Map = map;
            MapInstanceId = guid;
            TimeSpaces = new List<TimeSpace>();
            EntryEvents = new List<Tuple<EventActionType, object>>();
            _monsters = new ThreadSafeSortedList<long, MapMonster>();
            _mapMonsterIds = new List<int>();
            DroppedList = new ThreadSafeSortedList<long, MapItem>();
            _portals = new List<Portal>();
            UserShops = new Dictionary<long, MapShop>();
            _npcs = new List<MapNpc>();
            TimeSpaces = new List<TimeSpace>();
            _npcs.AddRange(ServerManager.Instance.GetMapNpcsByMapId(Map.MapId).AsEnumerable());
            StartLife();
        }

        #endregion

        #region Properties

        public ThreadSafeSortedList<long, MapItem> DroppedList { get; }

        public int DropRate { get; set; }

        public DateTime EndDate { get; set; }

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

        public bool Lock { get; set; }

        public Map Map { get; set; }

        public Guid MapInstanceId { get; set; }

        public MapInstanceType MapInstanceType { get; set; }

        public List<MapMonster> Monsters => _monsters.GetAllItems();

        public bool NpcEffectActivated { get; set; }

        public IEnumerable<MapNpc> Npcs => _npcs;

        public List<Portal> Portals => _portals;

        public List<TimeSpace> TimeSpaces { get; set; }

        public List<Tuple<EventActionType, object>> EntryEvents { get; set; }
        public bool ShopAllowed { get; set; }

        public Dictionary<long, MapShop> UserShops { get; }

        public int XpRate { get; set; }

        public byte MapIndexX { get; set; }

        public byte MapIndexY { get; set; }
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

                foreach (MapCell possibilitie in possibilities.OrderBy(s => ServerManager.RandomNumber()))
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

        public void LoadMonsters()
        {
            foreach (MapMonsterDTO monster in DAOFactory.MapMonsterDAO.LoadFromMap(Map.MapId).ToList())
            {
                _monsters[monster.MapMonsterId] = monster as MapMonster;
                _mapMonsterIds.Add(monster.MapMonsterId);
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
                    possibilities.Add(new GridPos { x = x, y = y });
                }
            }

            short mapX = 0;
            short mapY = 0;
            bool niceSpot = false;
            foreach (GridPos possibilitie in possibilities.OrderBy(s => _random.Next()))
            {
                mapX = (short)(session.Character.PositionX + possibilitie.x);
                mapY = (short)(session.Character.PositionY + possibilitie.y);
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
        public string GetClock()
        {
            return $"evnt 1 0 {(int)((EndDate - DateTime.Now).TotalSeconds * 10)} 1";
        }
        public IEnumerable<string> GeneratePlayerShopOnMap()
        {
            return UserShops.Select(shop => $"pflag 1 {shop.Value.OwnerId} {shop.Key + 1}").ToList();
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

        public void SetMapMapMonsterReference()
        {
            foreach (MapMonster monster in _monsters.GetAllItems())
            {
                monster.MapInstance = this;
            }
        }

        public void SetMapMapNpcReference()
        {
            foreach (MapNpc npc in _npcs)
            {
                npc.MapInstance = this;
                npc.JumpPointParameters = new JumpPointParam(Map.Grid, new GridPos(0, 0), new GridPos(0, 0), false, true, true, HeuristicMode.MANHATTAN);
            }
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

        public string GenerateRsfn(bool isInit = false)
        {
            if (MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                return $"rsfn {MapIndexX} {MapIndexY} {(isInit ? 1 : (Monsters?.Count == 0 ? 0 : 1))}";
            }
            return string.Empty;
        }

        internal void CreatePortal(Portal portal)
        {
            portal.SourceMapInstanceId = MapInstanceId;
            _portals.Add(portal);
            Sessions.Where(s => s.Character != null).ToList().ForEach(s => s.SendPacket(s.CurrentMapInstance.GenerateGp(portal)));
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

        internal string RunMapEvent(EventActionType eventaction, object param)
        {
            switch (eventaction)
            {
                case EventActionType.CLOCK:
                    EndDate = DateTime.Now.AddSeconds(Convert.ToDouble(param));
                    break;

                case EventActionType.DROPRATE:
                    DropRate = Convert.ToInt32(param);
                    break;

                case EventActionType.XPRATE:
                    XpRate = (int)param;
                    break;

                case EventActionType.DISPOSE:
                    Dispose();
                    break;

                case EventActionType.LOCK:
                    Lock = Convert.ToBoolean(param);
                    break;

                case EventActionType.MESSAGE:
                    Broadcast(Convert.ToString(param));
                    break;

                case EventActionType.SENDPACKET:
                    return Convert.ToString(param);

                case EventActionType.UNSPAWN:
                    UnspawnMonsters(Convert.ToInt32(param));
                    break;

                case EventActionType.SPAWN:
                    SummonMonsters((List<MonsterToSummon>)param);
                    break;

                case EventActionType.DROPITEMS:
                    DropItems((List<Tuple<short, int, short, short>>)param);
                    break;

                case EventActionType.SPAWNONLASTENTRY:

                    //TODO REVIEW THIS CASE
                    Character lastincharacter = Sessions.OrderByDescending(s => s.RegisterTime).FirstOrDefault()?.Character;
                    List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                    MapCell hornSpawn = new MapCell
                    {
                        X = lastincharacter?.PositionX ?? 154,
                        Y = lastincharacter?.PositionY ?? 140
                    };
                    long HornTarget = lastincharacter != null ? lastincharacter.CharacterId : -1;
                    summonParameters.Add(new MonsterToSummon(Convert.ToInt16(param), hornSpawn, HornTarget, true));
                    SummonMonsters(summonParameters);
                    break;
            }
            return string.Empty;
        }



        internal void StartMapEvent(TimeSpan timeSpan, EventActionType eventaction, object param)
        {
            Observable.Timer(timeSpan).Subscribe(x =>
            {
                RunMapEvent(eventaction, param);
            });
        }

        internal List<int> SummonMonsters(List<MonsterToSummon> summonParameters)
        {
            List<int> ids = new List<int>();
            foreach (MonsterToSummon mon in summonParameters)
            {
                NpcMonster npcmonster = ServerManager.GetNpc(mon.VNum);
                if (npcmonster != null)
                {
                    MapMonster monster = new MapMonster { MonsterVNum = npcmonster.NpcMonsterVNum, MapY = mon.SpawnCell.X, MapX = mon.SpawnCell.Y, MapId = Map.MapId, IsMoving = mon.IsMoving, MapMonsterId = GetNextMonsterId(), ShouldRespawn = false, Target = mon.Target };
                    monster.Initialize(this);
                    monster.StartLife();
                    AddMonster(monster);
                    Broadcast(monster.GenerateIn());
                    ids.Add(monster.MapMonsterId);
                }
            }

            return ids;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monsters.Dispose();
            }
        }

        public string GenerateGp(Portal portal)
        {
            return $"gp {portal.SourceX} {portal.SourceY} {ServerManager.GetMapInstance(portal.DestinationMapInstanceId)?.Map.MapId} {portal.Type} {Portals.Count} {(Portals.Contains(portal) ? (portal.IsDisabled ? 1 : 0) : 1)}";
        }

        #endregion
    }
}