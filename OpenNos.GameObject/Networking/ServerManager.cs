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
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ServerManager : BroadcastableBase, IDisposable
    {
        #region Members

        public bool ShutdownStop;

        private static readonly List<Item> _items = new List<Item>();

        private static readonly ConcurrentDictionary<Guid, MapInstance> _mapinstances = new ConcurrentDictionary<Guid, MapInstance>();

        private static readonly List<Map> _maps = new List<Map>();

        private static readonly List<NpcMonster> _npcs = new List<NpcMonster>();

        private static readonly List<Skill> _skills = new List<Skill>();

        private static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        private static ServerManager _instance;

        private static int seed = Environment.TickCount;

        private bool _disposed;

        private List<DropDTO> _generalDrops;

        public ThreadSafeSortedList<long, Group> GroupsThreadSafe;

        private long _lastGroupId;

        private ThreadSafeSortedList<short, List<MapNpc>> _mapNpcs;

        private ThreadSafeSortedList<short, List<DropDTO>> _monsterDrops;

        private ThreadSafeSortedList<short, List<NpcMonsterSkill>> _monsterSkills;

        private ThreadSafeSortedList<int, List<Recipe>> _recipes;

        private ThreadSafeSortedList<int, List<ShopItemDTO>> _shopItems;

        private ThreadSafeSortedList<int, Shop> _shops;

        private ThreadSafeSortedList<int, List<ShopSkillDTO>> _shopSkills;

        private ThreadSafeSortedList<int, List<TeleporterDTO>> _teleporters;

        private bool inRelationRefreshMode;

        #endregion

        #region Instantiation

        private ServerManager()
        {
            // do nothing
        }

        #endregion

        #region Properties

        public static ServerManager Instance => _instance ?? (_instance = new ServerManager());

        public MapInstance ArenaInstance { get; private set; }

        public List<BazaarItemLink> BazaarList { get; set; }

        public int ChannelId { get; set; }

        public List<CharacterRelationDTO> CharacterRelations { get; set; }

        public int DropRate { get; set; }

        public bool EventInWaiting { get; set; }

        public int FairyXpRate { get; set; }

        public MapInstance FamilyArenaInstance { get; private set; }

        public List<Family> FamilyList { get; set; }

        public int GoldDropRate { get; set; }

        public int GoldRate { get; set; }

        public List<Group> Groups => GroupsThreadSafe.GetAllItems();

        public int HeroicStartLevel { get; set; }

        public int HeroXpRate { get; set; }

        public bool InBazaarRefreshMode { get; set; }

        public bool InFamilyRefreshMode { get; set; }

        public List<MailDTO> Mails { get; private set; }

        public List<int> MateIds { get; internal set; } = new List<int>();

        public long MaxGold { get; set; }

        public byte MaxHeroLevel { get; set; }

        public byte MaxJobLevel { get; set; }

        public byte MaxLevel { get; set; }

        public byte MaxSPLevel { get; set; }

        public List<PenaltyLogDTO> PenaltyLogs { get; set; }

        public List<Schedule> Schedules { get; set; }

        public string ServerGroup { get; set; }

        public List<EventType> StartedEvents { get; set; }

        public Task TaskShutdown { get; set; }

        public List<CharacterDTO> TopComplimented { get; set; }

        public List<CharacterDTO> TopPoints { get; set; }

        public List<CharacterDTO> TopReputation { get; set; }

        public Guid WorldId { get; private set; }

        public int XPRate { get; set; }

        public List<Card> Cards { get; set; }

        public List<ScriptedInstance> Raids { get; set; }

        public List<Group> GroupList { get; set; } = new List<Group>();

        #endregion

        #region Methods

        public void AddGroup(Group group)
        {
            GroupsThreadSafe[group.GroupId] = group;
        }

        public void AskPVPRevive(long characterId)
        {
            ClientSession Session = GetSessionByCharacterId(characterId);
            if (Session != null && Session.HasSelectedCharacter)
            {
                if (Session.Character.IsVehicled)
                {
                    Session.Character.RemoveVehicle();
                }
                List<BuffType> bufftodisable = new List<BuffType>();
                bufftodisable.Add(BuffType.Bad);
                bufftodisable.Add(BuffType.Good);
                bufftodisable.Add(BuffType.Neutral);
                Session.Character.DisableBuffs(bufftodisable);
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateCond());
                Session.SendPackets(UserInterfaceHelper.Instance.GenerateVb());

                Session.SendPacket("eff_ob -1 -1 0 4269");
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^2 #revival^1 {Language.Instance.GetMessageFromKey("ASK_REVIVE_PVP")}"));
                Task.Factory.StartNew(async () =>
                {
                    bool revive = true;
                    for (int i = 1; i <= 30; i++)
                    {
                        await Task.Delay(1000);
                        if (Session.Character.Hp > 0)
                        {
                            revive = false;
                            break;
                        }
                    }
                    if (revive)
                    {
                        Instance.ReviveFirstPosition(Session.Character.CharacterId);
                    }
                });
            }
        }

        // PacketHandler -> with Callback?
        public void AskRevive(long characterId)
        {
            ClientSession Session = GetSessionByCharacterId(characterId);
            if (Session != null && Session.HasSelectedCharacter)
            {
                if (Session.Character.IsVehicled)
                {
                    Session.Character.RemoveVehicle();
                }
                List<BuffType> bufftodisable = new List<BuffType>();
                bufftodisable.Add(BuffType.Bad);
                bufftodisable.Add(BuffType.Good);
                bufftodisable.Add(BuffType.Neutral);
                Session.Character.DisableBuffs(bufftodisable);
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateCond());
                Session.SendPackets(UserInterfaceHelper.Instance.GenerateVb());
                switch (Session.CurrentMapInstance.MapInstanceType)
                {
                    case MapInstanceType.BaseMapInstance:
                        if (Session.Character.Level > 20)
                        {
                            Session.Character.Dignity -= (short)(Session.Character.Level < 50 ? Session.Character.Level : 50);
                            if (Session.Character.Dignity < -1000)
                            {
                                Session.Character.Dignity = -1000;
                            }
                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LOSE_DIGNITY"), (short)(Session.Character.Level < 50 ? Session.Character.Level : 50)), 11));
                            Session.SendPacket(Session.Character.GenerateFd());
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        }
                        Session.SendPacket("eff_ob -1 -1 0 4269");

                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^0 #revival^1 {(Session.Character.Level > 20 ? Language.Instance.GetMessageFromKey("ASK_REVIVE") : Language.Instance.GetMessageFromKey("ASK_REVIVE_FREE"))}"));
                        Task.Factory.StartNew(async () =>
                        {
                            bool revive = true;
                            for (int i = 1; i <= 30; i++)
                            {
                                await Task.Delay(1000);
                                if (Session.Character.Hp > 0)
                                {
                                    revive = false;
                                    break;
                                }
                            }
                            if (revive)
                            {
                                Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            }
                        });
                        break;

                    case MapInstanceType.TimeSpaceInstance:
                        if (!(Session.CurrentMapInstance.InstanceBag.Lives - Session.CurrentMapInstance.InstanceBag.DeadList.Count() <= 1))
                        {
                            Session.Character.Hp = 1;
                            Session.Character.Mp = 1;
                            return;
                        }
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("YOU_HAVE_LIFE"), Session.CurrentMapInstance.InstanceBag.Lives - Session.CurrentMapInstance.InstanceBag.DeadList.Count() + 1), 0));
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^1 #revival^1 {(Session.Character.Level > 10 ? Language.Instance.GetMessageFromKey("ASK_REVIVE_TS_LOW_LEVEL") : Language.Instance.GetMessageFromKey("ASK_REVIVE_TS"))}"));
                        Session.CurrentMapInstance.InstanceBag.DeadList.Add(Session.Character.CharacterId);
                        Task.Factory.StartNew(async () =>
                        {
                            bool revive = true;
                            for (int i = 1; i <= 30; i++)
                            {
                                await Task.Delay(1000);
                                if (Session.Character.Hp > 0)
                                {
                                    revive = false;
                                    break;
                                }
                            }
                            if (revive)
                            {
                                Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            }
                        });

                        break;

                    case MapInstanceType.RaidInstance:
                        List<long> save = Session.CurrentMapInstance.InstanceBag.DeadList.ConvertAll(s => s);
                        if (Session.CurrentMapInstance.InstanceBag.Lives - Session.CurrentMapInstance.InstanceBag.DeadList.Count() < 0)
                        {
                            Session.Character.Hp = 1;
                            Session.Character.Mp = 1;
                        }
                        else if (2 - save.Count(s => s == Session.Character.CharacterId) > 0)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("YOU_HAVE_LIFE_RAID"), 2 - Session.CurrentMapInstance.InstanceBag.DeadList.Where(s => s == Session.Character.CharacterId).Count())));
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("RAID_MEMBER_DEAD"), Session.Character.Name)));

                            Session.CurrentMapInstance.InstanceBag.DeadList.Add(Session.Character.CharacterId);
                            Session.Character.Group?.Characters.ForEach(
                            session =>
                            {
                                session.SendPacket(session.Character.Group.GeneraterRaidmbf());
                                session.SendPacket(session.Character.Group.GenerateRdlst());
                            });
                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(20000);
                                Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            });
                        }
                        else
                        {
                            Group grp = Session.Character.Group;
                            if (grp != null)
                            {
                                grp.Characters.ForEach(s =>
                                {
                                    s.SendPacket(s.Character.Group.GeneraterRaidmbf());
                                    s.SendPacket(s.Character.Group.GenerateRdlst());
                                });
                                grp.LeaveGroup(Session);
                                Session.SendPacket(Session.Character.GenerateRaid(1, true));
                                Session.SendPacket(Session.Character.GenerateRaid(2, true));
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("KICKED_FROM_RAID"), 0));
                            }
                        }
                        break;

                    case MapInstanceType.LodInstance:
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^0 #revival^1 {Language.Instance.GetMessageFromKey("ASK_REVIVE_LOD")}"));
                        Task.Factory.StartNew(async () =>
                        {
                            bool revive = true;
                            for (int i = 1; i <= 30; i++)
                            {
                                await Task.Delay(1000);
                                if (Session.Character.Hp > 0)
                                {
                                    revive = false;
                                    break;
                                }
                            }
                            if (revive)
                            {
                                Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            }
                        });
                        break;

                    default:
                        Instance.ReviveFirstPosition(Session.Character.CharacterId);
                        break;
                }
            }
        }

        public void BazaarRefresh(long BazaarItemId)
        {
            InBazaarRefreshMode = true;
            CommunicationServiceClient.Instance.UpdateBazaar(ServerGroup, BazaarItemId);
            SpinWait.SpinUntil(() => !InBazaarRefreshMode);
        }

        public void ChangeMap(long id, short? MapId = null, short? mapX = null, short? mapY = null)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session?.Character != null)
            {
                if (MapId != null)
                {
                    session.Character.MapInstanceId = GetBaseMapInstanceIdByMapId((short)MapId);
                }
                ChangeMapInstance(id, session.Character.MapInstanceId, mapX, mapY);
            }
        }

        // Both partly
        public void ChangeMapInstance(long id, Guid MapInstanceId, int? mapX = null, int? mapY = null)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session?.Character != null && !session.Character.IsChangingMapInstance)
            {
                try
                {
                    if (session.Character.IsExchanging)
                    {
                        session.Character.CloseExchangeOrTrade();
                    }
                    if (session.Character.HasShopOpened)
                    {
                        session.Character.CloseShop();
                    }
                    LeaveMap(session.Character.CharacterId);

                    session.Character.IsChangingMapInstance = true;

                    session.CurrentMapInstance.RemoveMonstersTarget(session.Character.CharacterId);
                    session.CurrentMapInstance.UnregisterSession(session.Character.CharacterId);

                    // cleanup sending queue to avoid sending uneccessary packets to it
                    session.ClearLowPriorityQueue();

                    session.Character.MapInstanceId = MapInstanceId;
                    if (session.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                    {
                        session.Character.MapId = session.Character.MapInstance.Map.MapId;
                        if (mapX != null && mapY != null)
                        {
                            session.Character.MapX = (short)mapX;
                            session.Character.MapY = (short)mapY;
                        }
                    }
                    if (mapX != null && mapY != null)
                    {
                        session.Character.PositionX = (short)mapX;
                        session.Character.PositionY = (short)mapY;
                    }

                    session.CurrentMapInstance = session.Character.MapInstance;
                    session.CurrentMapInstance.RegisterSession(session);

                    session.SendPacket(session.Character.GenerateCInfo());
                    session.SendPacket(session.Character.GenerateCMode());
                    session.SendPacket(session.Character.GenerateEq());
                    session.SendPacket(session.Character.GenerateEquipment());
                    session.SendPacket(session.Character.GenerateLev());
                    session.SendPacket(session.Character.GenerateStat());
                    session.SendPacket(session.Character.GenerateAt());
                    session.SendPacket(session.Character.GenerateCond());
                    session.SendPacket(session.Character.GenerateCMap());
                    session.SendPacket(session.Character.GenerateStatChar());
                    session.SendPacket(session.Character.GeneratePairy());
                    session.SendPacket(session.Character.GeneratePinit());
                    session.SendPackets(session.Character.GeneratePst());
                    session.SendPacket(session.Character.GenerateAct());
                    session.SendPacket(session.Character.GenerateScpStc());

                    Parallel.ForEach(session.CurrentMapInstance.Sessions.Where(s => s.Character != null && !s.Character.InvisibleGm), visibleSession =>
                    {
                        session.SendPacket(visibleSession.Character.GenerateIn());
                        session.SendPacket(visibleSession.Character.GenerateGidx());
                        visibleSession.Character.Mates.Where(m => m.IsTeamMember && m.CharacterId != session.Character.CharacterId).ToList().ForEach(mate =>
                        {
                            session.SendPacket(mate.GenerateIn());
                        });
                    });


                    session.SendPackets(session.CurrentMapInstance.GetMapItems());
                    MapInstancePortalHandler.GenerateMinilandEntryPortals(session.CurrentMapInstance.Map.MapId, session.Character.Miniland.MapInstanceId).ForEach(p => session.SendPacket(p.GenerateGp()));

                    if (session.CurrentMapInstance.InstanceBag.Clock.Enabled)
                    {
                        session.SendPacket(session.CurrentMapInstance.InstanceBag.Clock.GetClock());
                    }
                    if (session.CurrentMapInstance.Clock.Enabled)
                    {
                        session.SendPacket(session.CurrentMapInstance.InstanceBag.Clock.GetClock());
                    }

                    // TODO: fix this
                    if (session.Character.MapInstance.Map.MapTypes.Any(m => m.MapTypeId == (short)MapTypeEnum.CleftOfDarkness))
                    {
                        session.SendPacket("bc 0 0 0");
                    }
                    if (!session.Character.InvisibleGm)
                    {
                        Parallel.ForEach(session.Character.Mates.Where(m => m.IsTeamMember), mate =>
                        {
                            mate.PositionX = (short)(session.Character.PositionX + (mate.MateType == MateType.Partner ? -1 : 1));
                            mate.PositionY = (short)(session.Character.PositionY + 1);
                            session.CurrentMapInstance.Broadcast(mate.GenerateIn());
                        });
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    }
                    if (session.Character.Size != 10)
                    {
                        session.SendPacket(session.Character.GenerateScal());
                    }
                    if (session.CurrentMapInstance != null && session.CurrentMapInstance.IsDancing && !session.Character.IsDancing)
                    {
                        session.CurrentMapInstance?.Broadcast("dance 2");
                    }
                    else if (session.CurrentMapInstance != null && !session.CurrentMapInstance.IsDancing && session.Character.IsDancing)
                    {
                        session.Character.IsDancing = false;
                        session.CurrentMapInstance?.Broadcast("dance");
                    }
                    if (Groups != null)
                    {
                        Parallel.ForEach(Groups, group =>
                        {
                            foreach (ClientSession groupSession in group.Characters)
                            {
                                ClientSession chara = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId && s.CurrentMapInstance == groupSession.CurrentMapInstance);
                                if (chara == null)
                                {
                                    continue;
                                }
                                groupSession.SendPacket(groupSession.Character.GeneratePinit());
                                groupSession.SendPackets(groupSession.Character.GeneratePst());
                            }
                        });
                    }

                    if (session.Character.Group != null && session.Character.Group.GroupType == GroupType.Group)
                    {
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GeneratePidx(), ReceiverType.AllExceptMe);
                    }

                    session.Character.IsChangingMapInstance = false;
                    session.SendPacket(session.Character.GenerateMinimapPosition());
                    session.CurrentMapInstance.OnCharacterDiscoveringMapEvents.ForEach(e =>
                    {
                        if (!e.Item2.Contains(session.Character.CharacterId))
                        {
                            e.Item2.Add(session.Character.CharacterId);
                            EventHelper.Instance.RunEvent(e.Item1, session);
                        }
                    });
                }
                catch (Exception)
                {
                    Logger.Log.Warn("Character changed while changing map. Do not abuse Commands.");
                    session.Character.IsChangingMapInstance = false;
                }
            }
        }

        public override sealed void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public void FamilyRefresh(long FamilyId)
        {
            InFamilyRefreshMode = true;
            CommunicationServiceClient.Instance.UpdateFamily(ServerGroup, FamilyId);
            SpinWait.SpinUntil(() => !InFamilyRefreshMode);
        }

        public MapInstance GenerateMapInstance(short MapId, MapInstanceType type, InstanceBag mapclock)
        {
            Map map = _maps.FirstOrDefault(m => m.MapId.Equals(MapId));
            if (map != null)
            {
                Guid guid = Guid.NewGuid();
                MapInstance mapInstance = new MapInstance(map, guid, false, type, mapclock);
                mapInstance.LoadMonsters();
                mapInstance.LoadNpcs();
                mapInstance.LoadPortals();
                Parallel.ForEach(mapInstance.Monsters, mapMonster =>
                {
                    mapMonster.MapInstance = mapInstance;
                    mapInstance.AddMonster(mapMonster);
                });
                Parallel.ForEach(mapInstance.Npcs, mapNpc =>
                {
                    mapNpc.MapInstance = mapInstance;
                    mapInstance.AddNPC(mapNpc);
                });
                _mapinstances.TryAdd(guid, mapInstance);
                return mapInstance;
            }
            return null;
        }

        public IEnumerable<Skill> GetAllSkill()
        {
            return _skills;
        }

        public Guid GetBaseMapInstanceIdByMapId(short MapId)
        {
            return _mapinstances.FirstOrDefault(s => s.Value?.Map.MapId == MapId && s.Value.MapInstanceType == MapInstanceType.BaseMapInstance).Key;
        }

        public List<DropDTO> GetDropsByMonsterVNum(short monsterVNum)
        {
            return _monsterDrops.ContainsKey(monsterVNum) ? _generalDrops.Concat(_monsterDrops[monsterVNum]).ToList() : new List<DropDTO>();
        }

        public Group GetGroupByCharacterId(long characterId)
        {
            return Groups?.SingleOrDefault(g => g.IsMemberOfGroup(characterId));
        }

        public Item GetItem(short vnum)
        {
            return _items.FirstOrDefault(m => m.VNum.Equals(vnum));
        }

        public MapInstance GetMapInstance(Guid id)
        {
            return _mapinstances.ContainsKey(id) ? _mapinstances[id] : null;
        }

        public long GetNextGroupId()
        {
            _lastGroupId++;
            return _lastGroupId;
        }


        public NpcMonster GetNpc(short npcVNum)
        {
            return _npcs.FirstOrDefault(m => m.NpcMonsterVNum.Equals(npcVNum));
        }

        public T GetProperty<T>(string charName, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            if (session == null)
            {
                return default(T);
            }
            return (T)session.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetProperty<T>(long charId, string property)
        {
            ClientSession session = GetSessionByCharacterId(charId);
            if (session == null)
            {
                return default(T);
            }
            return (T)session.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public List<Recipe> GetReceipesByMapNpcId(int mapNpcId)
        {
            return _recipes.ContainsKey(mapNpcId) ? _recipes[mapNpcId] : new List<Recipe>();
        }

        public ClientSession GetSessionByCharacterName(string name)
        {
            return Sessions.SingleOrDefault(s => s.Character.Name == name);
        }

        public ClientSession GetSessionBySessionId(int sessionId)
        {
            return Sessions.SingleOrDefault(s => s.SessionId == sessionId);
        }

        public Skill GetSkill(short skillVNum)
        {
            return _skills.FirstOrDefault(m => m.SkillVNum.Equals(skillVNum));
        }

        public T GetUserMethod<T>(long characterId, string methodName)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session == null)
            {
                return default(T);
            }
            MethodInfo method = session.Character.GetType().GetMethod(methodName);

            return (T)method.Invoke(session.Character, null);
        }

        public void GroupLeave(ClientSession session)
        {
            if (Groups != null)
            {
                Group grp = Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId));
                if (grp != null)
                {
                    if ((grp.CharacterCount >= 3 && grp.GroupType == GroupType.Group) || (grp.CharacterCount >= 2 && grp.GroupType != GroupType.Group))
                    {
                        if (grp.Characters.ElementAt(0) == session)
                        {
                            Broadcast(session, UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_LEADER")), ReceiverType.OnlySomeone, string.Empty, grp.Characters.ElementAt(1).Character.CharacterId);
                        }
                        grp.LeaveGroup(session);

                        if (grp.GroupType == GroupType.Group)
                        {
                            foreach (ClientSession groupSession in grp.Characters)
                            {
                                groupSession.SendPacket(groupSession.Character.GeneratePinit());
                                groupSession.SendPackets(session.Character.GeneratePst());
                                groupSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LEAVE_GROUP"), session.Character.Name), 0));
                            }
                            session.SendPacket(session.Character.GeneratePinit());
                            session.SendPackets(session.Character.GeneratePst());
                            Broadcast(session.Character.GeneratePidx(true));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_LEFT"), 0));
                        }
                        else
                        {
                            foreach (ClientSession groupSession in grp.Characters)
                            {
                                session.SendPacket(session.Character.GenerateRaid(1, true));
                                session.SendPacket(session.Character.GenerateRaid(2, true));
                                groupSession.SendPacket(grp.GenerateRdlst());
                                groupSession.SendPacket(groupSession.Character.GenerateRaid(0, false));
                            }
                            if (session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                            {
                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId, session.Character.MapX, session.Character.MapY);
                            }
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("RAID_LEFT"), 0));
                        }

                    }
                    else
                    {
                        ClientSession[] grpmembers = new ClientSession[40];
                        grp.Characters.CopyTo(grpmembers);
                        foreach (ClientSession targetSession in grpmembers)
                        {
                            if (targetSession != null)
                            {
                                targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_CLOSED"), 0));
                                Broadcast(targetSession.Character.GeneratePidx(true));
                                grp.LeaveGroup(targetSession);
                                targetSession.SendPacket(targetSession.Character.GeneratePinit());
                                targetSession.SendPackets(targetSession.Character.GeneratePst());
                            }
                        }
                        GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                        GroupsThreadSafe.Remove(grp.GroupId);
                    }
                    session.Character.Group = null;
                }
            }
        }

        public void Initialize()
        {
            // parse rates
            XPRate = int.Parse(ConfigurationManager.AppSettings["RateXp"]);
            HeroXpRate = int.Parse(ConfigurationManager.AppSettings["RateHeroicXp"]);
            DropRate = int.Parse(ConfigurationManager.AppSettings["RateDrop"]);
            MaxGold = long.Parse(ConfigurationManager.AppSettings["MaxGold"]);
            GoldDropRate = int.Parse(ConfigurationManager.AppSettings["GoldRateDrop"]);
            GoldRate = int.Parse(ConfigurationManager.AppSettings["RateGold"]);
            FairyXpRate = int.Parse(ConfigurationManager.AppSettings["RateFairyXp"]);
            MaxLevel = byte.Parse(ConfigurationManager.AppSettings["MaxLevel"]);
            MaxJobLevel = byte.Parse(ConfigurationManager.AppSettings["MaxJobLevel"]);
            MaxSPLevel = byte.Parse(ConfigurationManager.AppSettings["MaxSPLevel"]);
            MaxHeroLevel = byte.Parse(ConfigurationManager.AppSettings["MaxHeroLevel"]);
            HeroicStartLevel = byte.Parse(ConfigurationManager.AppSettings["HeroicStartLevel"]);
            Schedules = ConfigurationManager.GetSection("eventScheduler") as List<Schedule>;
            Mails = DAOFactory.MailDAO.LoadAll().ToList();

            var itemPartitioner = Partitioner.Create(DAOFactory.ItemDAO.LoadAll(), EnumerablePartitionerOptions.NoBuffering);
            ThreadSafeSortedList<short, Item> _item = new ThreadSafeSortedList<short, Item>();
            Parallel.ForEach(itemPartitioner, new ParallelOptions { MaxDegreeOfParallelism = 4 }, itemDTO =>
            {
                switch (itemDTO.ItemType)
                {
                    case ItemType.Ammo:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Armor:
                        _item[itemDTO.VNum] = new WearableItem(itemDTO);
                        break;

                    case ItemType.Box:
                        _item[itemDTO.VNum] = new BoxItem(itemDTO);
                        break;

                    case ItemType.Event:
                        _item[itemDTO.VNum] = new MagicalItem(itemDTO);
                        break;

                    case ItemType.Fashion:
                        _item[itemDTO.VNum] = new WearableItem(itemDTO);
                        break;

                    case ItemType.Food:
                        _item[itemDTO.VNum] = new FoodItem(itemDTO);
                        break;

                    case ItemType.Jewelery:
                        _item[itemDTO.VNum] = new WearableItem(itemDTO);
                        break;

                    case ItemType.Magical:
                        _item[itemDTO.VNum] = new MagicalItem(itemDTO);
                        break;

                    case ItemType.Main:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Map:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Part:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Potion:
                        _item[itemDTO.VNum] = new PotionItem(itemDTO);
                        break;

                    case ItemType.Production:
                        _item[itemDTO.VNum] = new ProduceItem(itemDTO);
                        break;

                    case ItemType.Quest1:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Quest2:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Sell:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;

                    case ItemType.Shell:
                        _item[itemDTO.VNum] = new MagicalItem(itemDTO);
                        break;

                    case ItemType.Snack:
                        _item[itemDTO.VNum] = new SnackItem(itemDTO);
                        break;

                    case ItemType.Special:
                        _item[itemDTO.VNum] = new SpecialItem(itemDTO);
                        break;

                    case ItemType.Specialist:
                        _item[itemDTO.VNum] = new WearableItem(itemDTO);
                        break;

                    case ItemType.Teacher:
                        _item[itemDTO.VNum] = new TeacherItem(itemDTO);
                        break;

                    case ItemType.Upgrade:
                        _item[itemDTO.VNum] = new UpgradeItem(itemDTO);
                        break;

                    case ItemType.Weapon:
                        _item[itemDTO.VNum] = new WearableItem(itemDTO);
                        break;

                    default:
                        _item[itemDTO.VNum] = new NoFunctionItem(itemDTO);
                        break;
                }
            });
            _items.AddRange(_item.GetAllItems());
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ITEMS_LOADED"), _items.Count));

            // intialize monsterdrops
            _monsterDrops = new ThreadSafeSortedList<short, List<DropDTO>>();
            Parallel.ForEach(DAOFactory.DropDAO.LoadAll().GroupBy(d => d.MonsterVNum), monsterDropGrouping =>
            {
                if (monsterDropGrouping.Key.HasValue)
                {
                    _monsterDrops[monsterDropGrouping.Key.Value] = monsterDropGrouping.OrderBy(d => d.DropChance).ToList();
                }
                else
                {
                    _generalDrops = monsterDropGrouping.ToList();
                }
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("DROPS_LOADED"), _monsterDrops.GetAllItems().Sum(i => i.Count)));

            // initialize monsterskills
            _monsterSkills = new ThreadSafeSortedList<short, List<NpcMonsterSkill>>();
            Parallel.ForEach(DAOFactory.NpcMonsterSkillDAO.LoadAll().GroupBy(n => n.NpcMonsterVNum), monsterSkillGrouping =>
            {
                _monsterSkills[monsterSkillGrouping.Key] = monsterSkillGrouping.Select(n => n as NpcMonsterSkill).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MONSTERSKILLS_LOADED"), _monsterSkills.GetAllItems().Sum(i => i.Count)));

            // initialize Families
            LoadBazaar();
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("BAZAR_LOADED"), _monsterSkills.GetAllItems().Sum(i => i.Count)));

            // initialize npcmonsters
            ThreadSafeSortedList<short, NpcMonster> _npcMonsters = new ThreadSafeSortedList<short, NpcMonster>();
            Parallel.ForEach(DAOFactory.NpcMonsterDAO.LoadAll(), npcMonster =>
            {
                _npcMonsters[npcMonster.NpcMonsterVNum] = npcMonster as NpcMonster;
                _npcMonsters[npcMonster.NpcMonsterVNum].BCards = new List<BCard>();
                DAOFactory.BCardDAO.LoadByNpcMonsterVNum(npcMonster.NpcMonsterVNum).ToList().ForEach(s => _npcMonsters[npcMonster.NpcMonsterVNum].BCards.Add((BCard)s));
            });
            _npcs.AddRange(_npcMonsters.GetAllItems());
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_LOADED"), _npcs.Count));

            // intialize recipes
            _recipes = new ThreadSafeSortedList<int, List<Recipe>>();
            Parallel.ForEach(DAOFactory.RecipeDAO.LoadAll().GroupBy(r => r.MapNpcId), recipeGrouping =>
            {
                _recipes[recipeGrouping.Key] = recipeGrouping.Select(r => r as Recipe).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("RECIPES_LOADED"), _recipes.GetAllItems().Sum(i => i.Count)));

            // initialize shopitems
            _shopItems = new ThreadSafeSortedList<int, List<ShopItemDTO>>();
            Parallel.ForEach(DAOFactory.ShopItemDAO.LoadAll().GroupBy(s => s.ShopId), shopItemGrouping =>
            {
                _shopItems[shopItemGrouping.Key] = shopItemGrouping.ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_LOADED"), _shopItems.GetAllItems().Sum(i => i.Count)));

            // initialize shopskills
            _shopSkills = new ThreadSafeSortedList<int, List<ShopSkillDTO>>();
            Parallel.ForEach(DAOFactory.ShopSkillDAO.LoadAll().GroupBy(s => s.ShopId), shopSkillGrouping =>
            {
                _shopSkills[shopSkillGrouping.Key] = shopSkillGrouping.ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPSKILLS_LOADED"), _shopSkills.GetAllItems().Sum(i => i.Count)));

            // initialize shops
            _shops = new ThreadSafeSortedList<int, Shop>();
            Parallel.ForEach(DAOFactory.ShopDAO.LoadAll(), shopGrouping =>
            {
                _shops[shopGrouping.MapNpcId] = (Shop)shopGrouping;
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPS_LOADED"), _shops.GetAllItems().Count));

            // initialize teleporters
            _teleporters = new ThreadSafeSortedList<int, List<TeleporterDTO>>();
            Parallel.ForEach(DAOFactory.TeleporterDAO.LoadAll().GroupBy(t => t.MapNpcId), teleporterGrouping =>
            {
                _teleporters[teleporterGrouping.Key] = teleporterGrouping.Select(t => t).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TELEPORTERS_LOADED"), _teleporters.GetAllItems().Sum(i => i.Count)));

            // initialize skills
            ThreadSafeSortedList<short, Skill> _skill = new ThreadSafeSortedList<short, Skill>();
            Parallel.ForEach(DAOFactory.SkillDAO.LoadAll(), skill =>
            {
                Skill skillObj = skill as Skill;
                skillObj.Combos.AddRange(DAOFactory.ComboDAO.LoadBySkillVnum(skillObj.SkillVNum).ToList());
                skillObj.BCards = new List<BCard>();
                DAOFactory.BCardDAO.LoadBySkillVNum(skillObj.SkillVNum).ToList().ForEach(o => skillObj.BCards.Add((BCard)o));
                _skill[skillObj.SkillVNum] = skillObj as Skill;
            });
            _skills.AddRange(_skill.GetAllItems());
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SKILLS_LOADED"), _skills.Count));

            // initialize buffs
            Cards = new List<Card>();
            foreach (CardDTO carddto in DAOFactory.CardDAO.LoadAll())
            {
                Card card = (Card)carddto;
                card.BCards = new List<BCard>();
                DAOFactory.BCardDAO.LoadByCardId(card.CardId).ToList().ForEach(o => card.BCards.Add((BCard)o));
                Cards.Add(card);
            }


            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("CARDS_LOADED"), _skills.Count));

            // intialize mapnpcs
            _mapNpcs = new ThreadSafeSortedList<short, List<MapNpc>>();
            Parallel.ForEach(DAOFactory.MapNpcDAO.LoadAll().GroupBy(t => t.MapId), mapNpcGrouping =>
            {
                _mapNpcs[mapNpcGrouping.Key] = mapNpcGrouping.Select(t => t as MapNpc).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPNPCS_LOADED"), _mapNpcs.GetAllItems().Sum(i => i.Count)));

            try
            {
                int i = 0;
                int monstercount = 0;
                var mapPartitioner = Partitioner.Create(DAOFactory.MapDAO.LoadAll(), EnumerablePartitionerOptions.NoBuffering);
                ThreadSafeSortedList<short, Map> _mapList = new ThreadSafeSortedList<short, Map>();
                Parallel.ForEach(mapPartitioner, new ParallelOptions { MaxDegreeOfParallelism = 8 }, map =>
                {
                    Guid guid = Guid.NewGuid();
                    Map mapinfo = new Map(map.MapId, map.Data)
                    {
                        Music = map.Music
                    };
                    _mapList[map.MapId] = mapinfo;
                    MapInstance newMap = new MapInstance(mapinfo, guid, map.ShopAllowed, MapInstanceType.BaseMapInstance, new InstanceBag());
                    _mapinstances.TryAdd(guid, newMap);

                    Task.Run(() => newMap.LoadPortals());
                    newMap.LoadNpcs();
                    newMap.LoadMonsters();

                    Parallel.ForEach(newMap.Npcs, mapNpc =>
                    {
                        mapNpc.MapInstance = newMap;
                        newMap.AddNPC(mapNpc);
                    });
                    Parallel.ForEach(newMap.Monsters, mapMonster =>
                    {
                        mapMonster.MapInstance = newMap;
                        newMap.AddMonster(mapMonster);
                    });
                    monstercount += newMap.Monsters.Count;
                    i++;
                });
                _maps.AddRange(_mapList.GetAllItems());
                if (i != 0)
                {
                    Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPS_LOADED"), i));
                }
                else
                {
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));
                }
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPMONSTERS_LOADED"), monstercount));

                StartedEvents = new List<EventType>();
                LoadFamilies();
                LaunchEvents();
                RefreshRanking();
                CharacterRelations = DAOFactory.CharacterRelationDAO.LoadAll().ToList();
                PenaltyLogs = DAOFactory.PenaltyLogDAO.LoadAll().ToList();
                if (DAOFactory.MapDAO.LoadById(2006) != null)
                {
                    ArenaInstance = GenerateMapInstance(2006, MapInstanceType.NormalInstance, new InstanceBag());
                    ArenaInstance.IsPVP = true;
                }
                if (DAOFactory.MapDAO.LoadById(2106) != null)
                {
                    FamilyArenaInstance = GenerateMapInstance(2106, MapInstanceType.NormalInstance, new InstanceBag());
                    FamilyArenaInstance.IsPVP = true;
                }
                LoadScriptedInstances();
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }

            //Register the new created TCPIP server to the api
            Guid serverIdentification = Guid.NewGuid();
            WorldId = serverIdentification;
        }

        public bool IsCharacterMemberOfGroup(long characterId)
        {
            return Groups != null && Groups.Any(g => g.IsMemberOfGroup(characterId));
        }

        public bool IsCharactersGroupFull(long characterId)
        {
            return Groups != null && Groups.Any(g => g.IsMemberOfGroup(characterId) && g.CharacterCount == (byte)g.GroupType);
        }

        public void JoinMiniland(ClientSession Session, ClientSession MinilandOwner)
        {
            ChangeMapInstance(Session.Character.CharacterId, MinilandOwner.Character.Miniland.MapInstanceId, 5, 8);
            if (Session.Character.Miniland.MapInstanceId != MinilandOwner.Character.Miniland.MapInstanceId)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Session.Character.MinilandMessage.Replace(' ', '^'), 0));
                Session.SendPacket(Session.Character.GenerateMlinfobr());
                MinilandOwner.Character.GeneralLogs.Add(new GeneralLogDTO { AccountId = Session.Account.AccountId, CharacterId = Session.Character.CharacterId, IpAddress = Session.IpAddress, LogData = "Miniland", LogType = "World", Timestamp = DateTime.Now });
                Session.SendPacket(MinilandOwner.Character.GenerateMinilandObjectForFriends());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateMlinfo());
                Session.SendPacket(MinilandOwner.Character.GetMinilandObjectList());
            }
            MinilandOwner.Character.Mates.Where(s => !s.IsTeamMember).ToList().ForEach(s => Session.SendPacket(s.GenerateIn()));
            Session.SendPackets(MinilandOwner.Character.GetMinilandEffects());
            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MINILAND_VISITOR"), Session.Character.GeneralLogs.Count(s => s.LogData == "Miniland" && s.Timestamp.Day == DateTime.Now.Day), Session.Character.GeneralLogs.Count(s => s.LogData == "Miniland")), 10));
        }

        // Server
        public void Kick(string characterName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(characterName));
            session?.Disconnect();
        }

        // Map
        public void LeaveMap(long id)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session == null)
            {
                return;
            }
            session.SendPacket(UserInterfaceHelper.Instance.GenerateMapOut());
            session.Character.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s => session.CurrentMapInstance?.Broadcast(session, s.GenerateOut(), ReceiverType.AllExceptMe));
            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateOut(), ReceiverType.AllExceptMe);
        }


        public int RandomNumber(int min = 0, int max = 100)
        {
            return random.Value.Next(min, max);
        }

        public void RefreshRanking()
        {
            TopComplimented = DAOFactory.CharacterDAO.GetTopCompliment();
            TopPoints = DAOFactory.CharacterDAO.GetTopPoints();
            TopReputation = DAOFactory.CharacterDAO.GetTopReputation();
        }

        public void RelationRefresh(long RelationId)
        {
            inRelationRefreshMode = true;
            CommunicationServiceClient.Instance.UpdateRelation(ServerGroup, RelationId);
            SpinWait.SpinUntil(() => !inRelationRefreshMode);
        }

        public void RemoveMapInstance(Guid MapId)
        {
            KeyValuePair<Guid, MapInstance> map = _mapinstances.FirstOrDefault(s => s.Key == MapId);
            if (!map.Equals(default(KeyValuePair<Guid, MapInstance>)))
            {
                map.Value.Dispose();
                ((IDictionary)_mapinstances).Remove(map.Key);
            }
        }

        // Map
        public void ReviveFirstPosition(long characterId)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session != null && session.Character.Hp <= 0)
            {
                if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance || session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                {
                    session.Character.Hp = (int)session.Character.HPLoad();
                    session.Character.Mp = (int)session.Character.MPLoad();
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                    session.SendPacket(session.Character.GenerateStat());
                }
                else
                {
                    session.Character.Hp = 1;
                    session.Character.Mp = 1;
                    if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                    {
                        RespawnMapTypeDTO resp = session.Character.Respawn;
                        short x = (short)(resp.DefaultX + RandomNumber(-3, 3));
                        short y = (short)(resp.DefaultY + RandomNumber(-3, 3));
                        ChangeMap(session.Character.CharacterId, resp.DefaultMapId, x, y);
                    }
                    else
                    {
                        Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId, session.Character.MapX, session.Character.MapY);
                    }
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateTp());
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateRevive());
                    session.SendPacket(session.Character.GenerateStat());
                }
            }
        }

        public void SaveAll()
        {
            List<ClientSession> sessions = Sessions.Where(c => c.IsConnected).ToList();
            sessions.ForEach(s => s.Character?.Save());
            DAOFactory.BazaarItemDAO.RemoveOutDated();
        }

        public void SetProperty(long charId, string property, object value)
        {
            ClientSession session = GetSessionByCharacterId(charId);
            if (session == null)
            {
                return;
            }
            PropertyInfo propertyinfo = session.Character.GetType().GetProperty(property);
            propertyinfo.SetValue(session.Character, value, null);
        }

        public void Shout(string message)
        {
            Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Broadcast($"msg 2 {message}");
        }

        public async void ShutdownTask()
        {
            string message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 60 * 4; i++)
            {
                await Task.Delay(1000);
                if (Instance.ShutdownStop)
                {
                    Instance.ShutdownStop = false;
                    return;
                }
            }
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (Instance.ShutdownStop)
                {
                    Instance.ShutdownStop = false;
                    return;
                }
            }
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (Instance.ShutdownStop)
                {
                    Instance.ShutdownStop = false;
                    return;
                }
            }
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 10);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                if (Instance.ShutdownStop)
                {
                    Instance.ShutdownStop = false;
                    return;
                }
            }
            Instance.SaveAll();
            CommunicationServiceClient.Instance.UnregisterWorldServer(WorldId);
            Environment.Exit(0);
        }

        public void TeleportOnRandomPlaceInMap(ClientSession Session, Guid guid)
        {
            MapInstance map = GetMapInstance(guid);
            if (guid != default(Guid))
            {
                MapCell pos = map.Map.GetRandomPosition();
                ChangeMapInstance(Session.Character.CharacterId, guid, pos.X, pos.Y);
            }
        }

        // Server
        public void UpdateGroup(long charId)
        {
            try
            {
                if (Groups != null)
                {
                    Group myGroup = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId));
                    if (myGroup == null)
                    {
                        return;
                    }
                    ThreadSafeGenericList<ClientSession> groupMembers = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId))?.Characters;
                    if (groupMembers != null)
                    {
                        foreach (ClientSession session in groupMembers)
                        {
                            session.SendPacket(session.Character.GeneratePinit());
                            session.Character.Group.Characters.ForEach(s => session.SendPacket(s.Character.GenerateStat()));
                            session.SendPackets(session.Character.GeneratePst());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal List<NpcMonsterSkill> GetNpcMonsterSkillsByMonsterVNum(short npcMonsterVNum)
        {
            return _monsterSkills.ContainsKey(npcMonsterVNum) ? _monsterSkills[npcMonsterVNum] : new List<NpcMonsterSkill>();
        }

        internal Shop GetShopByMapNpcId(int mapNpcId)
        {
            return _shops.ContainsKey(mapNpcId) ? _shops[mapNpcId] : null;
        }

        internal List<ShopItemDTO> GetShopItemsByShopId(int shopId)
        {
            return _shopItems.ContainsKey(shopId) ? _shopItems[shopId] : new List<ShopItemDTO>();
        }

        internal List<ShopSkillDTO> GetShopSkillsByShopId(int shopId)
        {
            return _shopSkills.ContainsKey(shopId) ? _shopSkills[shopId] : new List<ShopSkillDTO>();
        }

        internal List<TeleporterDTO> GetTeleportersByNpcVNum(short npcMonsterVNum)
        {
            if (_teleporters != null && _teleporters.ContainsKey(npcMonsterVNum))
            {
                return _teleporters[npcMonsterVNum];
            }
            return new List<TeleporterDTO>();
        }

        internal void StopServer()
        {
            Instance.ShutdownStop = true;
            Instance.TaskShutdown = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monsterDrops.Dispose();
                GroupsThreadSafe.Dispose();
                _monsterSkills.Dispose();
                _shopSkills.Dispose();
                _shopItems.Dispose();
                _shops.Dispose();
                _recipes.Dispose();
                _mapNpcs.Dispose();
                _teleporters.Dispose();
            }
        }

        // Server
        private void BotProcess()
        {
            try
            {
                Shout(Language.Instance.GetMessageFromKey($"BOT_MESSAGE_{RandomNumber(0, 5)}"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void GroupProcess()
        {
            try
            {
                if (Groups != null)
                {
                    Parallel.ForEach(Groups, grp =>
                    {
                        foreach (ClientSession session in grp.Characters)
                        {
                            foreach (string str in grp.GeneratePst(session))
                            {
                                session.SendPacket(str);
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void LaunchEvents()
        {
            GroupsThreadSafe = new ThreadSafeSortedList<long, Group>();

            Observable.Interval(TimeSpan.FromMinutes(5)).Subscribe(x =>
            {
                SaveAllProcess();
            });

            Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(x =>
            {
                GroupProcess();
            });

            Observable.Interval(TimeSpan.FromHours(3)).Subscribe(x =>
            {
                BotProcess();
            });

            EventHelper.Instance.RunEvent(new EventContainer(Instance.GetMapInstance(Instance.GetBaseMapInstanceIdByMapId(98)), EventActionType.NPCSEFFECTCHANGESTATE, true));
            foreach (Schedule schedule in Schedules)
            {
                Observable.Timer(TimeSpan.FromSeconds(EventHelper.Instance.GetMilisecondsBeforeTime(schedule.Time).TotalSeconds), TimeSpan.FromDays(1)).Subscribe(e =>
                {
                    EventHelper.Instance.GenerateEvent(schedule.Event);
                });
            }

            Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(x =>
            {
                MailProcess();
            });

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x =>
            {
                RemoveItemProcess();
            });

            CommunicationServiceClient.Instance.SessionKickedEvent += OnSessionKicked;
            CommunicationServiceClient.Instance.MessageSentToCharacter += OnMessageSentToCharacter;
            CommunicationServiceClient.Instance.FamilyRefresh += OnFamilyRefresh;
            CommunicationServiceClient.Instance.RelationRefresh += OnRelationRefresh;
            CommunicationServiceClient.Instance.BazaarRefresh += OnBazaarRefresh;
            CommunicationServiceClient.Instance.PenaltyLogRefresh += OnPenaltyLogRefresh;
            CommunicationServiceClient.Instance.ShutdownEvent += OnShutdown;
            _lastGroupId = 1;
        }

        private void LoadBazaar()
        {
            BazaarList = new List<BazaarItemLink>();
            foreach (BazaarItemDTO bz in DAOFactory.BazaarItemDAO.LoadAll())
            {
                BazaarItemLink item = new BazaarItemLink
                {
                    BazaarItem = bz
                };
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadById(bz.SellerId);
                if (chara != null)
                {
                    item.Owner = chara.Name;
                    item.Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bz.ItemInstanceId);
                }
                BazaarList.Add(item);
            }
        }

        private void LoadFamilies()
        {
            // TODO: Parallelization of family load
            FamilyList = new List<Family>();
            ThreadSafeSortedList<long, Family> _family = new ThreadSafeSortedList<long, Family>();
            Parallel.ForEach(DAOFactory.FamilyDAO.LoadAll(), familyDTO =>
            {
                Family family = (Family)familyDTO;
                family.FamilyCharacters = new List<FamilyCharacter>();
                foreach (FamilyCharacterDTO famchar in DAOFactory.FamilyCharacterDAO.LoadByFamilyId(family.FamilyId).ToList())
                {
                    family.FamilyCharacters.Add((FamilyCharacter)famchar);
                }
                FamilyCharacter familyCharacter = family.FamilyCharacters.FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                if (familyCharacter != null)
                {
                    family.Warehouse = new Inventory((Character)familyCharacter.Character);
                    foreach (ItemInstanceDTO inventory in DAOFactory.IteminstanceDAO.LoadByCharacterId(familyCharacter.CharacterId).Where(s => s.Type == InventoryType.FamilyWareHouse).ToList())
                    {
                        inventory.CharacterId = familyCharacter.CharacterId;
                        family.Warehouse[inventory.Id] = (ItemInstance)inventory;
                    }
                }
                family.FamilyLogs = DAOFactory.FamilyLogDAO.LoadByFamilyId(family.FamilyId).ToList();
                _family[family.FamilyId] = family;
            });
            FamilyList.AddRange(_family.GetAllItems());
        }

        private void LoadScriptedInstances()
        {
            Raids = new List<ScriptedInstance>();
            Parallel.ForEach(_mapinstances, map =>
            {
                foreach (ScriptedInstance si in DAOFactory.ScriptedInstanceDAO.LoadByMap(map.Value.Map.MapId).ToList())
                {
                    if (si.Type == ScriptedInstanceType.TimeSpace)
                    {
                        si.LoadGlobals();
                        map.Value.ScriptedInstances.Add(si);
                    }
                    else if (si.Type == ScriptedInstanceType.Raid)
                    {
                        si.LoadGlobals();
                        Raids.Add(si);
                        Portal port = new Portal()
                        {
                            Type = (byte)PortalType.Raid,
                            SourceMapId = si.MapId,
                            SourceX = si.PositionX,
                            SourceY = si.PositionY
                        };
                        map.Value.Portals.Add(port);
                    }
                }
            });
        }


        private void MailProcess()
        {
            try
            {
                Mails = DAOFactory.MailDAO.LoadAll().ToList();
                Parallel.ForEach(Sessions.Where(c => c.IsConnected), session => session.Character?.RefreshMail()); // TODO: TEST!
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void OnBazaarRefresh(object sender, EventArgs e)
        {
            // TODO: Parallelization of bazaar.
            long BazaarId = (long)sender;
            BazaarItemDTO bzdto = DAOFactory.BazaarItemDAO.LoadById(BazaarId);
            BazaarItemLink bzlink = BazaarList.FirstOrDefault(s => s.BazaarItem.BazaarItemId == BazaarId);
            lock (BazaarList)
            {
                if (bzdto != null)
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadById(bzdto.SellerId);
                    if (bzlink != null)
                    {
                        BazaarList.Remove(bzlink);
                        bzlink.BazaarItem = bzdto;
                        bzlink.Owner = chara.Name;
                        bzlink.Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bzdto.ItemInstanceId);
                        BazaarList.Add(bzlink);
                    }
                    else
                    {
                        BazaarItemLink item = new BazaarItemLink
                        {
                            BazaarItem = bzdto
                        };
                        if (chara != null)
                        {
                            item.Owner = chara.Name;
                            item.Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bzdto.ItemInstanceId);
                        }
                        BazaarList.Add(item);
                    }
                }
                else if (bzlink != null)
                {
                    BazaarList.Remove(bzlink);
                }
            }
            InBazaarRefreshMode = false;
        }

        private void OnFamilyRefresh(object sender, EventArgs e)
        {
            // TODO: Parallelization of family.
            long FamilyId = (long)sender;
            FamilyDTO famdto = DAOFactory.FamilyDAO.LoadById(FamilyId);
            Family fam = FamilyList.FirstOrDefault(s => s.FamilyId == FamilyId);
            lock (FamilyList)
            {
                if (famdto != null)
                {
                    if (fam != null)
                    {
                        MapInstance lod = fam.LandOfDeath;
                        FamilyList.Remove(fam);
                        fam = (Family)famdto;
                        fam.FamilyCharacters = new List<FamilyCharacter>();
                        foreach (FamilyCharacterDTO famchar in DAOFactory.FamilyCharacterDAO.LoadByFamilyId(fam.FamilyId).ToList())
                        {
                            fam.FamilyCharacters.Add((FamilyCharacter)famchar);
                        }
                        FamilyCharacter familyCharacter = fam.FamilyCharacters.FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                        if (familyCharacter != null)
                        {
                            fam.Warehouse = new Inventory((Character)familyCharacter.Character);
                            foreach (ItemInstanceDTO inventory in DAOFactory.IteminstanceDAO.LoadByCharacterId(familyCharacter.CharacterId).Where(s => s.Type == InventoryType.FamilyWareHouse).ToList())
                            {
                                inventory.CharacterId = familyCharacter.CharacterId;
                                fam.Warehouse[inventory.Id] = (ItemInstance)inventory;
                            }
                        }
                        fam.FamilyLogs = DAOFactory.FamilyLogDAO.LoadByFamilyId(fam.FamilyId).ToList();
                        fam.LandOfDeath = lod;
                        FamilyList.Add(fam);
                    }
                    else
                    {
                        Family fami = (Family)famdto;
                        fami.FamilyCharacters = new List<FamilyCharacter>();
                        foreach (FamilyCharacterDTO famchar in DAOFactory.FamilyCharacterDAO.LoadByFamilyId(fami.FamilyId).ToList())
                        {
                            fami.FamilyCharacters.Add((FamilyCharacter)famchar);
                        }
                        FamilyCharacter familyCharacter = fami.FamilyCharacters.FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                        if (familyCharacter != null)
                        {
                            fami.Warehouse = new Inventory((Character)familyCharacter.Character);
                            foreach (ItemInstanceDTO inventory in DAOFactory.IteminstanceDAO.LoadByCharacterId(familyCharacter.CharacterId).Where(s => s.Type == InventoryType.FamilyWareHouse).ToList())
                            {
                                inventory.CharacterId = familyCharacter.CharacterId;
                                fami.Warehouse[inventory.Id] = (ItemInstance)inventory;
                            }
                        }
                        fami.FamilyLogs = DAOFactory.FamilyLogDAO.LoadByFamilyId(fami.FamilyId).ToList();
                        FamilyList.Add(fami);
                    }
                }
                else if (fam != null)
                {
                    FamilyList.Remove(fam);
                }
            }
            InFamilyRefreshMode = false;
        }

        private void OnMessageSentToCharacter(object sender, EventArgs e)
        {
            if (sender != null)
            {
                SCSCharacterMessage message = (SCSCharacterMessage)sender;

                ClientSession targetSession = Sessions.SingleOrDefault(s => s.Character.CharacterId == message.DestinationCharacterId);
                switch (message.Type)
                {
                    case MessageType.WhisperGM:
                    case MessageType.Whisper:
                        if (targetSession == null || message.Type == MessageType.WhisperGM && targetSession.Account.Authority != AuthorityType.GameMaster)
                        {
                            return;
                        }

                        if (targetSession.Character.GmPvtBlock)
                        {
                            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                            {
                                DestinationCharacterId = message.SourceCharacterId,
                                SourceCharacterId = message.DestinationCharacterId.Value,
                                SourceWorldId = WorldId,
                                Message = targetSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_CHAT_BLOCKED"), 10),
                                Type = MessageType.PrivateChat
                            });
                        }
                        else if (targetSession.Character.WhisperBlocked)
                        {
                            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                            {
                                DestinationCharacterId = message.SourceCharacterId,
                                SourceCharacterId = message.DestinationCharacterId.Value,
                                SourceWorldId = WorldId,
                                Message = UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_WHISPER_BLOCKED"), 0),
                                Type = MessageType.PrivateChat
                            });
                        }
                        else
                        {
                            if (message.SourceWorldId != WorldId)
                            {
                                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                                {
                                    DestinationCharacterId = message.SourceCharacterId,
                                    SourceCharacterId = message.DestinationCharacterId.Value,
                                    SourceWorldId = WorldId,
                                    Message = targetSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MESSAGE_SENT_TO_CHARACTER"), targetSession.Character.Name, ChannelId), 11),
                                    Type = MessageType.PrivateChat
                                });
                                targetSession.SendPacket($"{message.Message} <{Language.Instance.GetMessageFromKey("CHANNEL")}: {CommunicationServiceClient.Instance.GetChannelIdByWorldId(message.SourceWorldId)}>");
                            }
                            else
                            {
                                targetSession.SendPacket(message.Message);
                            }
                        }
                        break;

                    case MessageType.Shout:
                        Shout(message.Message);
                        break;

                    case MessageType.PrivateChat:
                        targetSession?.SendPacket(message.Message);
                        break;

                    case MessageType.FamilyChat:
                        if (message.DestinationCharacterId.HasValue)
                        {
                            if (message.SourceWorldId != WorldId)
                            {
                                Parallel.ForEach(Instance.Sessions, session =>
                                {
                                    if (session.HasSelectedCharacter && session.Character.Family != null)
                                    {
                                        if (session.Character.Family.FamilyId == message.DestinationCharacterId)
                                        {
                                            session.SendPacket($"say 1 0 6 <{Language.Instance.GetMessageFromKey("CHANNEL")}: {CommunicationServiceClient.Instance.GetChannelIdByWorldId(message.SourceWorldId)}>{message.Message}");
                                        }
                                    }
                                });
                            }
                        }
                        break;

                    case MessageType.Family:
                        if (message.DestinationCharacterId.HasValue)
                        {
                            Parallel.ForEach(Instance.Sessions, session =>
                            {
                                if (session.HasSelectedCharacter && session.Character.Family != null)
                                {
                                    if (session.Character.Family.FamilyId == message.DestinationCharacterId)
                                    {
                                        session.SendPacket(message.Message);
                                    }
                                }
                            });
                        }
                        break;
                }
            }
        }

        private void OnPenaltyLogRefresh(object sender, EventArgs e)
        {
            int relId = (int)sender;
            PenaltyLogDTO reldto = DAOFactory.PenaltyLogDAO.LoadById(relId);
            PenaltyLogDTO rel = PenaltyLogs.FirstOrDefault(s => s.PenaltyLogId == relId);
            if (reldto != null)
            {
                if (rel != null)
                {
                    rel = reldto;
                }
                else
                {
                    PenaltyLogs.Add(reldto);
                }
            }
            else if (rel != null)
            {
                PenaltyLogs.Remove(rel);
            }
        }

        private void OnRelationRefresh(object sender, EventArgs e)
        {
            inRelationRefreshMode = true;
            long relId = (long)sender;
            lock (CharacterRelations)
            {
                CharacterRelationDTO reldto = DAOFactory.CharacterRelationDAO.LoadById(relId);
                CharacterRelationDTO rel = CharacterRelations.FirstOrDefault(s => s.CharacterRelationId == relId);
                if (reldto != null)
                {
                    if (rel != null)
                    {
                        rel = reldto;
                    }
                    else
                    {
                        CharacterRelations.Add(reldto);
                    }
                }
                else if (rel != null)
                {
                    CharacterRelations.Remove(rel);
                }
            }
            inRelationRefreshMode = false;
        }

        private void OnSessionKicked(object sender, EventArgs e)
        {
            if (sender != null)
            {
                Tuple<long?, long?> kickedSession = (Tuple<long?, long?>)sender;

                ClientSession targetSession = Sessions.FirstOrDefault(s => (!kickedSession.Item1.HasValue || s.SessionId == kickedSession.Item1.Value)
                && (!kickedSession.Item1.HasValue || s.Account.AccountId == kickedSession.Item2));

                targetSession?.Disconnect();
            }
        }

        private void OnShutdown(object sender, EventArgs e)
        {
            if (Instance.TaskShutdown != null)
            {
                Instance.ShutdownStop = true;
                Instance.TaskShutdown = null;
            }
            else
            {
                Instance.TaskShutdown = new Task(Instance.ShutdownTask);
                Instance.TaskShutdown.Start();
            }
        }

        private void RemoveItemProcess()
        {
            try
            {
                Parallel.ForEach(Sessions.Where(c => c.IsConnected), session => session.Character?.RefreshValidity());
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        // Server
        private void SaveAllProcess()
        {
            try
            {
                Logger.Log.Info(Language.Instance.GetMessageFromKey("SAVING_ALL"));
                SaveAll();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
    }
}
