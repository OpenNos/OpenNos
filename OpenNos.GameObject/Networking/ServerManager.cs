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

        public ConcurrentDictionary<long, Group> GroupsThreadSafe;

        private long _lastGroupId;

        private ConcurrentDictionary<short, List<MapNpc>> _mapNpcs;

        private ConcurrentDictionary<short, List<DropDTO>> _monsterDrops;

        private ConcurrentDictionary<short, List<NpcMonsterSkill>> _monsterSkills;

        private ConcurrentDictionary<int, List<Recipe>> _recipes;

        private ConcurrentDictionary<int, List<ShopItemDTO>> _shopItems;

        private ConcurrentDictionary<int, Shop> _shops;

        private ConcurrentDictionary<int, List<ShopSkillDTO>> _shopSkills;

        private ConcurrentDictionary<int, List<TeleporterDTO>> _teleporters;


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

        public List<ConcurrentBag<ArenaTeamMember>> ArenaTeams { get; set; } = new List<ConcurrentBag<ArenaTeamMember>>();

        public int ChannelId { get; set; }

        public List<CharacterRelationDTO> CharacterRelations { get; set; }

        public int DropRate { get; set; }

        public bool EventInWaiting { get; set; }

        public int FairyXpRate { get; set; }

        public MapInstance FamilyArenaInstance { get; private set; }

        public List<Family> FamilyList { get; set; }

        public int GoldDropRate { get; set; }

        public int GoldRate { get; set; }

        public int ReputRate { get; set; }

        public List<Group> Groups
        {
            get { return GroupsThreadSafe.Select(s => s.Value).ToList(); }
        }

        public int HeroicStartLevel { get; set; }

        public int HeroXpRate { get; set; }

        public bool IceBreakerInWaiting { get; set; }

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

        public List<ArenaMember> ArenaMembers { get; set; } = new List<ArenaMember>();

        public MapInstance Act4ShipDemon { get; set; }

        public MapInstance Act4ShipAngel { get; set; }

        public List<MapInstance> Act4Maps { get; set; }

        public Act4Stat Act4AngelStat { get; set; }

        public Act4Stat Act4DemonStat { get; set; }

        public DateTime Act4RaidStart { get; set; }

        public int AccountLimit { get; set; }

        public string IpAddress { get; set; }

        public short Port { get; set; }

        #endregion

        #region Methods

        public void AddGroup(Group group)
        {
            GroupsThreadSafe[group.GroupId] = group;
        }

        public void AskPVPRevive(long characterId)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session == null || !session.HasSelectedCharacter)
            {
                return;
            }
            if (session.Character.IsVehicled)
            {
                session.Character.RemoveVehicle();
            }
            List<BuffType> bufftodisable = new List<BuffType> { BuffType.Bad, BuffType.Good, BuffType.Neutral };
            session.Character.DisableBuffs(bufftodisable);
            session.SendPacket(session.Character.GenerateStat());
            session.SendPacket(session.Character.GenerateCond());
            session.SendPackets(UserInterfaceHelper.Instance.GenerateVb());
            session.SendPacket("eff_ob -1 -1 0 4269");
            switch (session.CurrentMapInstance.MapInstanceType)
            {
                case MapInstanceType.TalentArenaMapInstance:
                    ConcurrentBag<ArenaTeamMember> team = Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == session));
                    ArenaTeamMember member = team?.FirstOrDefault(s => s.Session == session);
                    if (member != null)
                    {
                        if (member.LastSummoned == null)
                        {
                            session.CurrentMapInstance.InstanceBag.DeadList.Add(session.Character.CharacterId);
                            member.Dead = true;
                            team.ToList().Where(s => s.LastSummoned != null).ToList().ForEach(s =>
                            {
                                s.LastSummoned = null;
                                s.Session.Character.PositionX = s.ArenaTeamType == ArenaTeamType.ERENIA ? (short)120 : (short)19;
                                s.Session.Character.PositionY = s.ArenaTeamType == ArenaTeamType.ERENIA ? (short)39 : (short)40;
                                session.CurrentMapInstance.Broadcast(s.Session.Character.GenerateTp());
                                s.Session.SendPacket(UserInterfaceHelper.Instance.GenerateTaSt(TalentArenaOptionType.Watch));
                            });
                            ArenaTeamMember killer = team.OrderBy(s => s.Order).FirstOrDefault(s => !s.Dead && s.ArenaTeamType != member.ArenaTeamType);
                            session.CurrentMapInstance.Broadcast(session.Character.GenerateSay(string.Format("TEAM_WINNER_ARENA_ROUND", killer?.Session.Character.Name, killer?.ArenaTeamType), 10));
                            session.CurrentMapInstance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format("TEAM_WINNER_ARENA_ROUND", killer?.Session.Character.Name, killer?.ArenaTeamType), 0));
                            session.CurrentMapInstance.Sessions.Except(team.Where(s => s.ArenaTeamType == killer?.ArenaTeamType).Select(s => s.Session)).ToList().ForEach(o =>
                            {
                                if (killer?.ArenaTeamType == ArenaTeamType.ERENIA)
                                {
                                    o.SendPacket(killer.Session.Character.GenerateTaM(2));
                                    o.SendPacket(killer.Session.Character.GenerateTaP(2, true));
                                }
                                else
                                {
                                    o.SendPacket(member.Session.Character.GenerateTaM(2));
                                    o.SendPacket(member.Session.Character.GenerateTaP(2, true));
                                }
                                o.SendPacket($"taw_d {member.Session.Character.CharacterId}");
                                o.SendPacket(member.Session.Character.GenerateSay(string.Format("WINNER_ARENA_ROUND", killer?.Session.Character.Name, killer?.ArenaTeamType, member.Session.Character.Name), 10));
                                o.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format("WINNER_ARENA_ROUND", killer?.Session.Character.Name, killer?.ArenaTeamType, member.Session.Character.Name), 0));
                            });
                        }

                        member.Session.Character.PositionX = member.ArenaTeamType == ArenaTeamType.ERENIA ? (short)120 : (short)19;
                        member.Session.Character.PositionY = member.ArenaTeamType == ArenaTeamType.ERENIA ? (short)39 : (short)40;
                        session.CurrentMapInstance.Broadcast(member.Session, member.Session.Character.GenerateTp());
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateTaSt(TalentArenaOptionType.Watch));
                        team.Where(friends => friends.ArenaTeamType == member.ArenaTeamType).ToList().ForEach(friends => { friends.Session.SendPacket(friends.Session.Character.GenerateTaFc(0)); });
                        team.ToList().ForEach(arenauser =>
                        {
                            arenauser.Session.SendPacket(arenauser.Session.Character.GenerateTaP(2, true));
                            arenauser.Session.SendPacket(arenauser.Session.Character.GenerateTaM(2));
                        });

                        session.Character.Hp = (int)session.Character.HPLoad();
                        session.Character.Mp = (int)session.Character.MPLoad();
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                        session.SendPacket(session.Character.GenerateStat());
                    }
                    break;

                default:
                    session.Character.LeaveTalentArena(true);
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^2 #revival^1 {Language.Instance.GetMessageFromKey("ASK_REVIVE_PVP")}"));
                    Task.Factory.StartNew(async () =>
                    {
                        bool revive = true;
                        for (int i = 1; i <= 30; i++)
                        {
                            await Task.Delay(1000);
                            if (session.Character.Hp <= 0)
                            {
                                continue;
                            }
                            revive = false;
                            break;
                        }
                        if (revive)
                        {
                            Instance.ReviveFirstPosition(session.Character.CharacterId);
                        }
                    });
                    break;
            }
        }

        // PacketHandler -> with Callback?
        public void AskRevive(long characterId)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session == null || !session.HasSelectedCharacter || session.CurrentMapInstance == null)
            {
                return;
            }
            if (session.Character.IsVehicled)
            {
                session.Character.RemoveVehicle();
            }
            List<BuffType> bufftodisable = new List<BuffType> { BuffType.Bad, BuffType.Good, BuffType.Neutral };
            session.Character.DisableBuffs(bufftodisable);
            session.SendPacket(session.Character.GenerateStat());
            session.SendPacket(session.Character.GenerateCond());
            session.SendPackets(UserInterfaceHelper.Instance.GenerateVb());
            switch (session.CurrentMapInstance.MapInstanceType)
            {
                case MapInstanceType.BaseMapInstance:
                    if (session.Character.Level > 20)
                    {
                        session.Character.Dignity -= (short)(session.Character.Level < 50 ? session.Character.Level : 50);
                        if (session.Character.Dignity < -1000)
                        {
                            session.Character.Dignity = -1000;
                        }
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LOSE_DIGNITY"), (short)(session.Character.Level < 50 ? session.Character.Level : 50)), 11));
                        session.SendPacket(session.Character.GenerateFd());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    }
                    session.SendPacket("eff_ob -1 -1 0 4269");

                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^0 #revival^1 {(session.Character.Level > 20 ? Language.Instance.GetMessageFromKey("ASK_REVIVE") : Language.Instance.GetMessageFromKey("ASK_REVIVE_FREE"))}"));
                    Task.Factory.StartNew(async () =>
                    {
                        bool revive = true;
                        for (int i = 1; i <= 30; i++)
                        {
                            await Task.Delay(1000);
                            if (session.Character.Hp <= 0)
                            {
                                continue;
                            }
                            revive = false;
                            break;
                        }
                        if (revive)
                        {
                            Instance.ReviveFirstPosition(session.Character.CharacterId);
                        }
                    });
                    break;

                case MapInstanceType.TimeSpaceInstance:
                    if (!(session.CurrentMapInstance.InstanceBag.Lives - session.CurrentMapInstance.InstanceBag.DeadList.Count() <= 1))
                    {
                        session.Character.Hp = 1;
                        session.Character.Mp = 1;
                        return;
                    }
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("YOU_HAVE_LIFE"), session.CurrentMapInstance.InstanceBag.Lives - session.CurrentMapInstance.InstanceBag.DeadList.Count() + 1), 0));
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^1 #revival^1 {(session.Character.Level > 10 ? Language.Instance.GetMessageFromKey("ASK_REVIVE_TS_LOW_LEVEL") : Language.Instance.GetMessageFromKey("ASK_REVIVE_TS"))}"));
                    session.CurrentMapInstance.InstanceBag.DeadList.Add(session.Character.CharacterId);
                    Task.Factory.StartNew(async () =>
                    {
                        bool revive = true;
                        for (int i = 1; i <= 30; i++)
                        {
                            await Task.Delay(1000);
                            if (session.Character.Hp <= 0)
                            {
                                continue;
                            }
                            revive = false;
                            break;
                        }
                        if (revive)
                        {
                            Instance.ReviveFirstPosition(session.Character.CharacterId);
                        }
                    });

                    break;

                case MapInstanceType.RaidInstance:
                    List<long> save = session.CurrentMapInstance.InstanceBag.DeadList.ToList();
                    if (session.CurrentMapInstance.InstanceBag.Lives - session.CurrentMapInstance.InstanceBag.DeadList.Count() < 0)
                    {
                        session.Character.Hp = 1;
                        session.Character.Mp = 1;
                    }
                    else if (2 - save.Count(s => s == session.Character.CharacterId) > 0)
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("YOU_HAVE_LIFE_RAID"), 2 - session.CurrentMapInstance.InstanceBag.DeadList.Count(s => s == session.Character.CharacterId))));
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("RAID_MEMBER_DEAD"), session.Character.Name)));

                        session.CurrentMapInstance.InstanceBag.DeadList.Add(session.Character.CharacterId);
                        session.Character.Group?.Characters.ToList().ForEach(player =>
                        {
                            player.SendPacket(player.Character.Group.GeneraterRaidmbf());
                            player.SendPacket(player.Character.Group.GenerateRdlst());
                        });
                        Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(20000);
                            Instance.ReviveFirstPosition(session.Character.CharacterId);
                        });
                    }
                    else
                    {
                        Group grp = session.Character.Group;
                        if (grp != null)
                        {
                            grp.Characters.ToList().ForEach(s =>
                            {
                                s.SendPacket(s.Character.Group.GeneraterRaidmbf());
                                s.SendPacket(s.Character.Group.GenerateRdlst());
                            });
                            grp.LeaveGroup(session);
                            session.SendPacket(session.Character.GenerateRaid(1, true));
                            session.SendPacket(session.Character.GenerateRaid(2, true));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("KICKED_FROM_RAID"), 0));
                        }
                    }
                    break;

                case MapInstanceType.LodInstance:
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#revival^0 #revival^1 {Language.Instance.GetMessageFromKey("ASK_REVIVE_LOD")}"));
                    Task.Factory.StartNew(async () =>
                    {
                        bool revive = true;
                        for (int i = 1; i <= 30; i++)
                        {
                            await Task.Delay(1000);
                            if (session.Character.Hp <= 0)
                            {
                                continue;
                            }
                            revive = false;
                            break;
                        }
                        if (revive)
                        {
                            Instance.ReviveFirstPosition(session.Character.CharacterId);
                        }
                    });
                    break;

                default:
                    Instance.ReviveFirstPosition(session.Character.CharacterId);
                    break;
            }
        }

        public void BazaarRefresh(long bazaarItemId)
        {
            InBazaarRefreshMode = true;
            CommunicationServiceClient.Instance.UpdateBazaar(ServerGroup, bazaarItemId);
            SpinWait.SpinUntil(() => !InBazaarRefreshMode);
        }

        public void ChangeMap(long id, short? mapId = null, short? mapX = null, short? mapY = null)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session?.Character == null)
            {
                return;
            }
            if (mapId != null)
            {
                session.Character.MapInstanceId = GetBaseMapInstanceIdByMapId((short)mapId);
            }
            try
            {
                KeyValuePair<Guid, MapInstance> unused = _mapinstances.First(x => x.Key == session.Character.MapInstanceId);
            }
            catch
            {
                return;
            }
            ChangeMapInstance(id, session.Character.MapInstanceId, mapX, mapY);
        }

        // Both partly
        public void ChangeMapInstance(long id, Guid mapInstanceId, int? mapX = null, int? mapY = null)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session?.Character == null || session.Character.IsChangingMapInstance)
            {
                return;
            }
            try
            {
                if (session.Character.IsExchanging || session.Character.InExchangeOrTrade)
                {
                    session.Character.CloseExchangeOrTrade();
                }
                if (session.Character.HasShopOpened)
                {
                    session.Character.CloseShop();
                }
                session.Character.LeaveTalentArena();
                session.CurrentMapInstance.RemoveMonstersTarget(session.Character.CharacterId);
                session.CurrentMapInstance.UnregisterSession(session.Character.CharacterId);
                LeaveMap(session.Character.CharacterId);
                session.Character.IsChangingMapInstance = true;
                if (session.Character.IsSitting)
                {
                    session.Character.IsSitting = false;
                }
                // cleanup sending queue to avoid sending uneccessary packets to it
                session.ClearLowPriorityQueue();

                session.Character.MapInstanceId = mapInstanceId;
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

                if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4Instance)
                {
                    session.SendPacket(session.Character.GenerateFc());

                    if (mapInstanceId == session.Character.Family?.Act4Raid?.MapInstanceId || mapInstanceId == session.Character.Family?.Act4RaidBossMap?.MapInstanceId)
                    {
                        session.SendPacket(session.Character.GenerateDG());
                    }
                }

                session.SendPacket(session.CurrentMapInstance.GenerateMapDesignObjects());
                session.SendPackets(session.CurrentMapInstance.GetMapDesignObjectEffects());

                Parallel.ForEach(session.CurrentMapInstance.Sessions.Where(s => s.Character?.InvisibleGm == false), visibleSession =>
                {
                    if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.Act4Instance || session.Character.Faction == visibleSession.Character.Faction)
                    {
                        session.SendPacket(visibleSession.Character.GenerateIn());
                        session.SendPacket(visibleSession.Character.GenerateGidx());
                        visibleSession.Character.Mates.Where(m => m.IsTeamMember && m.CharacterId != session.Character.CharacterId).ToList().ForEach(mate =>
                        {
                            session.SendPacket(mate.GenerateIn(false, session.CurrentMapInstance.MapInstanceType.Equals(MapInstanceType.Act4Instance)));
                        });
                    }
                    else
                    {
                        session.SendPacket(visibleSession.Character.GenerateIn(true));
                        visibleSession.Character.Mates.Where(m => m.IsTeamMember && m.CharacterId != session.Character.CharacterId).ToList().ForEach(m =>
                        {
                            session.SendPacket(m.GenerateIn(true, true));
                        });
                    }
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
                    });

                    Parallel.ForEach(session.CurrentMapInstance.Sessions.Where(s => s.Character != null), s =>
                    {
                        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.Act4Instance || session.Character.Faction == s.Character.Faction)
                        {
                            s.SendPacket(session.Character.GenerateIn());
                            s.SendPacket(session.Character.GenerateGidx());
                            session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m =>
                            {
                                s.SendPacket(m.GenerateIn());
                            });
                        }
                        else
                        {
                            s.SendPacket(session.Character.GenerateIn(true));
                            session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m =>
                            {
                                s.SendPacket(m.GenerateIn(true));
                            });
                        }
                    });
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
                        foreach (ClientSession groupSession in @group.Characters)
                        {
                            ClientSession chara = Sessions.FirstOrDefault(s =>
                                s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId && s.CurrentMapInstance == groupSession.CurrentMapInstance);
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
                    if (e.Item2.Contains(session.Character.CharacterId))
                    {
                        return;
                    }
                    e.Item2.Add(session.Character.CharacterId);
                    EventHelper.Instance.RunEvent(e.Item1, session);
                });
            }
            catch (Exception ex)
            {
                Logger.Log.Warn("Character changed while changing map. Do not abuse Commands.");
                session.Character.IsChangingMapInstance = false;
            }
        }

        public sealed override void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        public void FamilyRefresh(long familyId)
        {
            CommunicationServiceClient.Instance.UpdateFamily(ServerGroup, familyId);
        }

        public MapInstance GenerateMapInstance(short mapId, MapInstanceType type, InstanceBag mapclock)
        {
            Map map = _maps.FirstOrDefault(m => m.MapId.Equals(mapId));
            if (map == null)
            {
                return null;
            }
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

        public IEnumerable<MapInstance> GetMapInstancesByMapInstanceType(MapInstanceType type)
        {
            return _mapinstances.Values.Where(s => s.MapInstanceType == type);
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
            if (Groups == null)
            {
                return;
            }
            Group grp = Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId));
            if (grp == null)
            {
                return;
            }
            if (grp.CharacterCount >= 3 && grp.GroupType == GroupType.Group || grp.CharacterCount >= 2 && grp.GroupType != GroupType.Group)
            {
                if (grp.Characters.ElementAt(0) == session)
                {
                    Broadcast(session, UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_LEADER")), ReceiverType.OnlySomeone, string.Empty,
                        grp.Characters.ElementAt(1).Character.CharacterId);
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
                        Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId, session.Character.MapX, session.Character.MapY);
                    }
                    session?.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("RAID_LEFT"), 0));
                }

            }
            else
            {
                ClientSession[] grpmembers = new ClientSession[40];
                grp.Characters.ToList().CopyTo(grpmembers);
                foreach (ClientSession targetSession in grpmembers)
                {
                    if (targetSession == null)
                    {
                        continue;
                    }
                    targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_CLOSED"), 0));
                    Broadcast(targetSession.Character.GeneratePidx(true));
                    grp.LeaveGroup(targetSession);
                    targetSession.SendPacket(targetSession.Character.GeneratePinit());
                    targetSession.SendPackets(targetSession.Character.GeneratePst());
                }
                GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                GroupsThreadSafe.TryRemove(grp.GroupId, out Group value);
            }
            if (session != null)
            {
                session.Character.Group = null;
            }
        }

        public void Initialize()
        {
            // parse rates
            XPRate = int.Parse(ConfigurationManager.AppSettings["RateXp"]);
            HeroXpRate = int.Parse(ConfigurationManager.AppSettings["RateXpHero"]);
            FairyXpRate = int.Parse(ConfigurationManager.AppSettings["RateXpFairy"]);
            ReputRate = int.Parse(ConfigurationManager.AppSettings["RateReput"]);
            DropRate = int.Parse(ConfigurationManager.AppSettings["RateDrop"]);
            MaxGold = long.Parse(ConfigurationManager.AppSettings["MaxGold"]);
            GoldDropRate = int.Parse(ConfigurationManager.AppSettings["GoldRateDrop"]);
            GoldRate = int.Parse(ConfigurationManager.AppSettings["RateGold"]);
            MaxLevel = byte.Parse(ConfigurationManager.AppSettings["MaxLevel"]);
            MaxJobLevel = byte.Parse(ConfigurationManager.AppSettings["MaxJobLevel"]);
            MaxSPLevel = byte.Parse(ConfigurationManager.AppSettings["MaxSPLevel"]);
            MaxHeroLevel = byte.Parse(ConfigurationManager.AppSettings["MaxHeroLevel"]);
            HeroicStartLevel = byte.Parse(ConfigurationManager.AppSettings["HeroicStartLevel"]);
            Schedules = ConfigurationManager.GetSection("eventScheduler") as List<Schedule>;
            Mails = DAOFactory.MailDAO.LoadAll().ToList();
            Act4RaidStart = DateTime.Now;
            Act4AngelStat = new Act4Stat();
            Act4DemonStat = new Act4Stat();

            OrderablePartitioner<ItemDTO> itemPartitioner = Partitioner.Create(DAOFactory.ItemDAO.LoadAll(), EnumerablePartitionerOptions.NoBuffering);
            ConcurrentDictionary<short, Item> item = new ConcurrentDictionary<short, Item>();
            Parallel.ForEach(itemPartitioner, new ParallelOptions { MaxDegreeOfParallelism = 4 }, itemDto =>
            {
                switch (itemDto.ItemType)
                {
                    case ItemType.Ammo:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Armor:
                        item[itemDto.VNum] = new WearableItem(itemDto);
                        break;

                    case ItemType.Box:
                        item[itemDto.VNum] = new BoxItem(itemDto);
                        break;

                    case ItemType.Event:
                        item[itemDto.VNum] = new MagicalItem(itemDto);
                        break;

                    case ItemType.Fashion:
                        item[itemDto.VNum] = new WearableItem(itemDto);
                        break;

                    case ItemType.Food:
                        item[itemDto.VNum] = new FoodItem(itemDto);
                        break;

                    case ItemType.Jewelery:
                        item[itemDto.VNum] = new WearableItem(itemDto);
                        break;

                    case ItemType.Magical:
                        item[itemDto.VNum] = new MagicalItem(itemDto);
                        break;

                    case ItemType.Main:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Map:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Part:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Potion:
                        item[itemDto.VNum] = new PotionItem(itemDto);
                        break;

                    case ItemType.Production:
                        item[itemDto.VNum] = new ProduceItem(itemDto);
                        break;

                    case ItemType.Quest1:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Quest2:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Sell:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;

                    case ItemType.Shell:
                        item[itemDto.VNum] = new MagicalItem(itemDto);
                        break;

                    case ItemType.Snack:
                        item[itemDto.VNum] = new SnackItem(itemDto);
                        break;

                    case ItemType.Special:
                        item[itemDto.VNum] = new SpecialItem(itemDto);
                        break;

                    case ItemType.Specialist:
                        item[itemDto.VNum] = new WearableItem(itemDto);
                        break;

                    case ItemType.Teacher:
                        item[itemDto.VNum] = new TeacherItem(itemDto);
                        break;

                    case ItemType.Upgrade:
                        item[itemDto.VNum] = new UpgradeItem(itemDto);
                        break;

                    case ItemType.Weapon:
                        item[itemDto.VNum] = new WearableItem(itemDto);
                        break;

                    default:
                        item[itemDto.VNum] = new NoFunctionItem(itemDto);
                        break;
                }
            });
            _items.AddRange(item.Select(s => s.Value));
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ITEMS_LOADED"), _items.Count));

            // intialize monsterdrops
            _monsterDrops = new ConcurrentDictionary<short, List<DropDTO>>();
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
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("DROPS_LOADED"), _monsterDrops.Sum(i => i.Value.Count)));

            // initialize monsterskills
            _monsterSkills = new ConcurrentDictionary<short, List<NpcMonsterSkill>>();
            Parallel.ForEach(DAOFactory.NpcMonsterSkillDAO.LoadAll().GroupBy(n => n.NpcMonsterVNum), monsterSkillGrouping =>
            {
                _monsterSkills[monsterSkillGrouping.Key] = monsterSkillGrouping.Select(n => n as NpcMonsterSkill).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MONSTERSKILLS_LOADED"), _monsterSkills.Sum(i => i.Value.Count)));

            // initialize Families
            LoadBazaar();
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("BAZAR_LOADED"), _monsterSkills.Sum(i => i.Value.Count)));

            // initialize npcmonsters
            ConcurrentDictionary<short, NpcMonster> npcMonsters = new ConcurrentDictionary<short, NpcMonster>();
            Parallel.ForEach(DAOFactory.NpcMonsterDAO.LoadAll(), npcMonster =>
            {
                npcMonsters[npcMonster.NpcMonsterVNum] = npcMonster as NpcMonster;
                NpcMonster monster = npcMonsters[npcMonster.NpcMonsterVNum];
                if (monster != null)
                {
                    monster.BCards = new List<BCard>();
                }
                DAOFactory.BCardDAO.LoadByNpcMonsterVNum(npcMonster.NpcMonsterVNum).ToList().ForEach(s => npcMonsters[npcMonster.NpcMonsterVNum].BCards.Add((BCard)s));
            });
            _npcs.AddRange(npcMonsters.Select(s => s.Value));
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_LOADED"), _npcs.Count));

            // intialize recipes
            _recipes = new ConcurrentDictionary<int, List<Recipe>>();
            Parallel.ForEach(DAOFactory.RecipeDAO.LoadAll().GroupBy(r => r.MapNpcId), recipeGrouping =>
            {
                _recipes[recipeGrouping.Key] = recipeGrouping.Select(r => r as Recipe).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("RECIPES_LOADED"), _recipes.Sum(i => i.Value.Count)));

            // initialize shopitems
            _shopItems = new ConcurrentDictionary<int, List<ShopItemDTO>>();
            Parallel.ForEach(DAOFactory.ShopItemDAO.LoadAll().GroupBy(s => s.ShopId), shopItemGrouping =>
            {
                _shopItems[shopItemGrouping.Key] = shopItemGrouping.ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_LOADED"), _shopItems.Sum(i => i.Value.Count)));

            // initialize shopskills
            _shopSkills = new ConcurrentDictionary<int, List<ShopSkillDTO>>();
            Parallel.ForEach(DAOFactory.ShopSkillDAO.LoadAll().GroupBy(s => s.ShopId), shopSkillGrouping =>
            {
                _shopSkills[shopSkillGrouping.Key] = shopSkillGrouping.ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPSKILLS_LOADED"), _shopSkills.Sum(i => i.Value.Count)));

            // initialize shops
            _shops = new ConcurrentDictionary<int, Shop>();
            Parallel.ForEach(DAOFactory.ShopDAO.LoadAll(), shopGrouping =>
            {
                _shops[shopGrouping.MapNpcId] = (Shop)shopGrouping;
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPS_LOADED"), _shops.Count));

            // initialize teleporters
            _teleporters = new ConcurrentDictionary<int, List<TeleporterDTO>>();
            Parallel.ForEach(DAOFactory.TeleporterDAO.LoadAll().GroupBy(t => t.MapNpcId), teleporterGrouping =>
            {
                _teleporters[teleporterGrouping.Key] = teleporterGrouping.Select(t => t).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TELEPORTERS_LOADED"), _teleporters.Sum(i => i.Value.Count)));

            // initialize skills
            ConcurrentDictionary<short, Skill> _skill = new ConcurrentDictionary<short, Skill>();
            Parallel.ForEach(DAOFactory.SkillDAO.LoadAll(), skill =>
            {
                if (!(skill is Skill skillObj))
                {
                    return;
                }
                skillObj.Combos.AddRange(DAOFactory.ComboDAO.LoadBySkillVnum(skillObj.SkillVNum).ToList());
                skillObj.BCards = new List<BCard>();
                DAOFactory.BCardDAO.LoadBySkillVNum(skillObj.SkillVNum).ToList().ForEach(o => skillObj.BCards.Add((BCard)o));
                _skill[skillObj.SkillVNum] = (Skill)skillObj;
            });
            _skills.AddRange(_skill.Select(s => s.Value));
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
            _mapNpcs = new ConcurrentDictionary<short, List<MapNpc>>();
            Parallel.ForEach(DAOFactory.MapNpcDAO.LoadAll().GroupBy(t => t.MapId), mapNpcGrouping =>
            {
                _mapNpcs[mapNpcGrouping.Key] = mapNpcGrouping.Select(t => t as MapNpc).ToList();
            });
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPNPCS_LOADED"), _mapNpcs.Sum(i => i.Value.Count)));

            try
            {
                int i = 0;
                int monstercount = 0;
                OrderablePartitioner<MapDTO> mapPartitioner = Partitioner.Create(DAOFactory.MapDAO.LoadAll(), EnumerablePartitionerOptions.NoBuffering);
                ConcurrentDictionary<short, Map> _mapList = new ConcurrentDictionary<short, Map>();
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
                _maps.AddRange(_mapList.Select(s => s.Value));
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
                    Logger.Log.Info("[ARENA] Arena Map Loaded");
                    ArenaInstance = GenerateMapInstance(2006, MapInstanceType.ArenaInstance, new InstanceBag());
                    ArenaInstance.IsPVP = true;
                    ArenaInstance.Portals.Add(new Portal
                    {
                        DestinationMapId = 1,
                        DestinationX = 1,
                        DestinationY = 1,
                        SourceMapId = 2006,
                        SourceX = 37,
                        SourceY = 15,
                    });
                }
                if (DAOFactory.MapDAO.LoadById(2106) != null)
                {
                    Logger.Log.Info("[ARENA] Family Arena Map Loaded");
                    FamilyArenaInstance = GenerateMapInstance(2106, MapInstanceType.ArenaInstance, new InstanceBag());
                    FamilyArenaInstance.IsPVP = true;
                    ArenaInstance.Portals.Add(new Portal
                    {
                        DestinationMapId = 1,
                        DestinationX = 1,
                        DestinationY = 1,
                        SourceMapId = 2106,
                        SourceX = 38,
                        SourceY = 3,
                    });
                }
                if (DAOFactory.MapDAO.LoadById(148) != null)
                {
                    Logger.Log.Info("[ACT4] Demon Ship Loaded");
                    Act4ShipDemon = GenerateMapInstance(148, MapInstanceType.ArenaInstance, null);
                    Logger.Log.Info("[ACT4] Angel Ship Loaded");
                    Act4ShipAngel = GenerateMapInstance(148, MapInstanceType.NormalInstance, null);
                }
                if (Act4Maps == null)
                {
                    Act4Maps = new List<MapInstance>();
                }
                foreach (Map m in _maps.Where(s => s.MapTypes.Any(o => o.MapTypeId == (short)MapTypeEnum.Act4 || o.MapTypeId == (short)MapTypeEnum.Act42)))
                {
                    MapInstance act4Map = GenerateMapInstance(m.MapId, MapInstanceType.Act4Instance, new InstanceBag());
                    if (act4Map.Map.MapId == 153)
                    {
                        act4Map.Portals.Clear();
                        // ANGEL
                        act4Map.Portals.Add(new Portal {DestinationMapId = 134, DestinationX = 46, DestinationY = 171, SourceMapId = 153, IsDisabled = false, Type = (short) PortalType.MapPortal});
                        // DEMON
                        act4Map.Portals.Add(new Portal {DestinationMapId = 134, DestinationX = 50, DestinationY = 171, SourceMapId = 153, IsDisabled = false, Type = (short) PortalType.MapPortal});
                    }
                    // TODO REMOVE THAT FOR RELEASE
                    if (act4Map.Map.MapId == 134)
                    {
                        Portal portal = act4Map.Portals.FirstOrDefault(s => s.DestinationMapId == 153);
                        if (portal != null)
                        {
                            portal.SourceX = 140;
                            portal.SourceY = 186;
                        }
                    }
                    act4Map.IsPVP = true;
                    Act4Maps.Add(act4Map);
                }

                foreach (MapInstance m in Act4Maps)
                {
                    foreach (Portal portal in m.Portals)
                    {
                        MapInstance mapInstance = Act4Maps.FirstOrDefault(s => s.Map.MapId == portal.DestinationMapId);
                        if (mapInstance != null)
                        {
                            portal.DestinationMapInstanceId = mapInstance.MapInstanceId;
                        }
                        else
                        {
                            m.Portals.RemoveAll(s => s.DestinationMapId == portal.DestinationMapId);
                            Logger.Log.Error($"Could not find Act4Map with Id {portal.DestinationMapId}");
                        }
                    }
                }
                Logger.Log.Info($"[ACT4] Initialized");
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

        public void JoinMiniland(ClientSession session, ClientSession minilandOwner)
        {
            ChangeMapInstance(session.Character.CharacterId, minilandOwner.Character.Miniland.MapInstanceId, 5, 8);
            if (session.Character.Miniland.MapInstanceId != minilandOwner.Character.Miniland.MapInstanceId)
            {
                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(session.Character.MinilandMessage.Replace(' ', '^'), 0));
                session.SendPacket(session.Character.GenerateMlinfobr());
                minilandOwner.Character.GeneralLogs.Add(new GeneralLogDTO { AccountId = session.Account.AccountId, CharacterId = session.Character.CharacterId, IpAddress = session.IpAddress, LogData = "Miniland", LogType = "World", Timestamp = DateTime.Now });
            }
            else
            {
                session.SendPacket(session.Character.GenerateMlinfo());
            }
            minilandOwner.Character.Mates.Where(s => !s.IsTeamMember).ToList().ForEach(s => session.SendPacket(s.GenerateIn()));
            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MINILAND_VISITOR"), session.Character.GeneralLogs.Count(s => s.LogData == "Miniland" && s.Timestamp.Day == DateTime.Now.Day), session.Character.GeneralLogs.Count(s => s.LogData == "Miniland")), 10));
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

        public void RelationRefresh(long relationId)
        {
            inRelationRefreshMode = true;
            CommunicationServiceClient.Instance.UpdateRelation(ServerGroup, relationId);
            SpinWait.SpinUntil(() => !inRelationRefreshMode);
        }

        public void RemoveMapInstance(Guid mapId)
        {
            KeyValuePair<Guid, MapInstance> map = _mapinstances.FirstOrDefault(s => s.Key == mapId);
            if (map.Equals(default(KeyValuePair<Guid, MapInstance>)))
            {
                return;
            }
            map.Value.Dispose();
            ((IDictionary)_mapinstances).Remove(map.Key);
        }

        // Map
        public void ReviveFirstPosition(long characterId)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session == null || session.Character.Hp > 0)
            {
                return;
            }
            short x;
            short y;
            switch (session.CurrentMapInstance.MapInstanceType)
            {
                case MapInstanceType.TimeSpaceInstance:
                case MapInstanceType.RaidInstance:
                    session.Character.Hp = (int) session.Character.HPLoad();
                    session.Character.Mp = (int) session.Character.MPLoad();
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRevive());
                    session.SendPacket(session.Character.GenerateStat());
                    break;
                case MapInstanceType.Act4Instance:
                    x = (short) (39 + Instance.RandomNumber(-2, 3));
                    y = (short) (42 + Instance.RandomNumber(-2, 3));
                    MapInstance citadel = Instance.Act4Maps.FirstOrDefault(s => s.Map.MapId == (session.Character.Faction == FactionType.Angel ? 130 : 131));
                    if (citadel != null)
                    {
                        Instance.ChangeMapInstance(session.Character.CharacterId, citadel.MapInstanceId, x, y);
                    }
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateTp());
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateRevive());
                    session.SendPacket(session.Character.GenerateStat());
                    break;
                default:
                    session.Character.Hp = 1;
                    session.Character.Mp = 1;
                    if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                    {
                        RespawnMapTypeDTO resp = session.Character.Respawn;
                        x = (short) (resp.DefaultX + RandomNumber(-3, 3));
                        y = (short) (resp.DefaultY + RandomNumber(-3, 3));
                        ChangeMap(session.Character.CharacterId, resp.DefaultMapId, x, y);
                    }
                    else
                    {
                        Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId, session.Character.MapX, session.Character.MapY);
                    }
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateTp());
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateRevive());
                    session.SendPacket(session.Character.GenerateStat());
                    break;
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
            propertyinfo?.SetValue(session.Character, value, null);
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
                if (!Instance.ShutdownStop)
                {
                    continue;
                }
                Instance.ShutdownStop = false;
                return;
            }
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (!Instance.ShutdownStop)
                {
                    continue;
                }
                Instance.ShutdownStop = false;
                return;
            }
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (!Instance.ShutdownStop)
                {
                    continue;
                }
                Instance.ShutdownStop = false;
                return;
            }
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 10);
            Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(message, 2));
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                if (!Instance.ShutdownStop)
                {
                    continue;
                }
                Instance.ShutdownStop = false;
                return;
            }
            Instance.SaveAll();
            CommunicationServiceClient.Instance.UnregisterWorldServer(WorldId);
            Environment.Exit(0);
        }

        public void TeleportOnRandomPlaceInMap(ClientSession session, Guid guid)
        {
            MapInstance map = GetMapInstance(guid);
            if (guid == default(Guid))
            {
                return;
            }
            MapCell pos = map.Map.GetRandomPosition();
            ChangeMapInstance(session.Character.CharacterId, guid, pos.X, pos.Y);
        }

        // Server
        public void UpdateGroup(long charId)
        {
            try
            {
                Group myGroup = Groups?.FirstOrDefault(s => s.IsMemberOfGroup(charId));
                if (myGroup == null)
                {
                    return;
                }
                ConcurrentBag<ClientSession> groupMembers = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId))?.Characters;
                if (groupMembers == null)
                {
                    return;
                }
                foreach (ClientSession session in groupMembers)
                {
                    session.SendPacket(session.Character.GeneratePinit());
                    session.SendPackets(session.Character.GeneratePst());
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
            GroupsThreadSafe = new ConcurrentDictionary<long, Group>();

            Observable.Interval(TimeSpan.FromMinutes(5)).Subscribe(x => { SaveAllProcess(); });

            Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(x => { Act4Process(); });

            Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(x => { GroupProcess(); });

            Observable.Interval(TimeSpan.FromMinutes(1)).Subscribe(x => { Act4FlowerProcess(); });

            Observable.Interval(TimeSpan.FromHours(3)).Subscribe(x => { BotProcess(); });

            Observable.Interval(TimeSpan.FromSeconds(90)).Subscribe(x => { MailProcess(); });

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x => { RemoveItemProcess(); });

            EventHelper.Instance.RunEvent(new EventContainer(Instance.GetMapInstance(Instance.GetBaseMapInstanceIdByMapId(98)), EventActionType.NPCSEFFECTCHANGESTATE, true));
            foreach (Schedule schedule in Schedules)
            {
                Observable.Timer(TimeSpan.FromSeconds(EventHelper.Instance.GetMilisecondsBeforeTime(schedule.Time).TotalSeconds), TimeSpan.FromDays(1)).Subscribe(e =>
                {
                    EventHelper.Instance.GenerateEvent(schedule.Event);
                });
            }


            EventHelper.Instance.GenerateEvent(EventType.ACT4SHIP);

            CommunicationServiceClient.Instance.SessionKickedEvent += OnSessionKicked;
            CommunicationServiceClient.Instance.MessageSentToCharacter += OnMessageSentToCharacter;
            CommunicationServiceClient.Instance.FamilyRefresh += OnFamilyRefresh;
            CommunicationServiceClient.Instance.RelationRefresh += OnRelationRefresh;
            CommunicationServiceClient.Instance.BazaarRefresh += OnBazaarRefresh;
            CommunicationServiceClient.Instance.PenaltyLogRefresh += OnPenaltyLogRefresh;
            CommunicationServiceClient.Instance.ShutdownEvent += OnShutdown;
            _lastGroupId = 1;
        }

        private void Act4FlowerProcess()
        {
            // FIND THE REAL VALUES
            foreach (MapInstance map in Act4Maps.Where(s => s.Map.MapId != 131 && s.Map.MapId != 130 && s.Npcs.Count(o => o.NpcVNum == 2004 && o.IsOut) < 7))
            {
                // TODO PROPERTY
                IEnumerable<MapNpc> npcs = map.Npcs.Where(s => s.IsOut);
                foreach (MapNpc i in npcs)
                {
                    MapCell randomPos = map.Map.GetRandomPosition();
                    i.MapX = randomPos.X;
                    i.MapY = randomPos.Y;
                    i.MapInstance.Broadcast(i.GenerateIn());
                }
            }
        }

        private void Act4Process()
        {
            MapInstance angelMapInstance = GetMapInstance(GetBaseMapInstanceIdByMapId(132));
            MapInstance demonMapInstance = GetMapInstance(GetBaseMapInstanceIdByMapId(133));

            if (angelMapInstance == null || demonMapInstance == null)
            {
                return;
            }

            void SummonMukraju(MapInstance instance, byte faction)
            {
                MapMonster monster = new MapMonster
                {
                    MonsterVNum = 556,
                    MapY = faction == 1 ? (short) 92 : (short) 95,
                    MapX = faction == 1 ? (short) 114 : (short) 20,
                    MapId = (short) (131 + faction),
                    IsMoving = true,
                    MapMonsterId = instance.GetNextMonsterId(),
                    ShouldRespawn = false
                };
                monster.Initialize(instance);
                instance.AddMonster(monster);
                instance.Broadcast(monster.GenerateIn());
            }

            Act4RaidType CreateRaid(FactionType faction)
            {
                IEnumerable<MapInstance> maps = Instance.GetMapInstancesByMapInstanceType(MapInstanceType.Act4Instance);
                Act4RaidType raid = (Act4RaidType) random.Value.Next(0, 5);
                //MapInstance middleAct4Map = maps?.FirstOrDefault(s => s.Map.MapId == );
                return raid;
            }

            if (Act4AngelStat.Percentage > 10000)
            {
                Act4AngelStat.Mode = 1;
                Act4AngelStat.Percentage = 0;
                Act4AngelStat.TotalTime = 300;
                SummonMukraju(angelMapInstance, 1);
            }

            if (Act4AngelStat.Mode == 1 && angelMapInstance.Monsters.All(s => s.MonsterVNum != 556))
            {
                Act4AngelStat.Mode = 3;
                Act4AngelStat.TotalTime = 3600;

                switch (CreateRaid(FactionType.Angel))
                {
                    case Act4RaidType.Morcos:
                        Act4AngelStat.IsMorcos = true;
                        break;
                    case Act4RaidType.Hatus:
                        Act4AngelStat.IsHatus = true;
                        break;
                    case Act4RaidType.Calvina:
                        Act4AngelStat.IsCalvina = true;
                        break;
                    case Act4RaidType.Berios:
                        Act4AngelStat.IsBerios = true;
                        break;
                }
            }

            if (Act4DemonStat.Percentage > 10000)
            {
                Act4DemonStat.Mode = 1;
                Act4DemonStat.Percentage = 0;
                Act4DemonStat.TotalTime = 300;
                SummonMukraju(demonMapInstance, 2);
            }

            if (Act4DemonStat.Mode == 1 && demonMapInstance.Monsters.All(s => s.MonsterVNum != 556))
            {
                Act4DemonStat.Mode = 3;
                Act4DemonStat.TotalTime = 3600;

                switch (CreateRaid(FactionType.Demon))
                {
                    case Act4RaidType.Morcos:
                        Act4AngelStat.IsMorcos = true;
                        break;
                    case Act4RaidType.Hatus:
                        Act4AngelStat.IsHatus = true;
                        break;
                    case Act4RaidType.Calvina:
                        Act4AngelStat.IsCalvina = true;
                        break;
                    case Act4RaidType.Berios:
                        Act4AngelStat.IsBerios = true;
                        break;
                }
            }

            Parallel.ForEach(Sessions.Where(s => s?.Character != null && s.CurrentMapInstance?.MapInstanceType == MapInstanceType.Act4Instance), sess => sess.SendPacket(sess.Character.GenerateFc()));
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
            ConcurrentDictionary<long, Family> families = new ConcurrentDictionary<long, Family>();
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
                families[family.FamilyId] = family;
            });
            FamilyList.AddRange(families.Select(s => s.Value));
        }

        private void LoadScriptedInstances()
        {
            Raids = new List<ScriptedInstance>();
            Parallel.ForEach(_mapinstances, map =>
            {
                foreach (ScriptedInstanceDTO scriptedInstanceDto in DAOFactory.ScriptedInstanceDAO.LoadByMap(map.Value.Map.MapId).ToList())
                {
                    ScriptedInstance si = (ScriptedInstance)scriptedInstanceDto;
                    switch (si.Type)
                    {
                        case ScriptedInstanceType.TimeSpace:
                            si.LoadGlobals();
                            map.Value.ScriptedInstances.Add(si);
                            break;
                        case ScriptedInstanceType.Raid:
                            si.LoadGlobals();
                            Raids.Add(si);
                            Portal port = new Portal
                            {
                                Type = (byte)PortalType.Raid,
                                SourceMapId = si.MapId,
                                SourceX = si.PositionX,
                                SourceY = si.PositionY
                            };
                            map.Value.Portals.Add(port);
                            break;
                    }
                }
            });
        }


        private void MailProcess()
        {
            try
            {
                Mails = DAOFactory.MailDAO.LoadAll().ToList();
                Parallel.ForEach(Sessions.Where(c => c.IsConnected), session => session.Character?.RefreshMail());
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void OnBazaarRefresh(object sender, EventArgs e)
        {
            // TODO: Parallelization of bazaar.
            long bazaarId = (long)sender;
            BazaarItemDTO bzdto = DAOFactory.BazaarItemDAO.LoadById(bazaarId);
            BazaarItemLink bzlink = BazaarList.FirstOrDefault(s => s.BazaarItem.BazaarItemId == bazaarId);
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
            long familyId = (long)sender;
            FamilyDTO famdto = DAOFactory.FamilyDAO.LoadById(familyId);
            Family fam = FamilyList.FirstOrDefault(s => s.FamilyId == familyId);
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
            if (sender == null)
            {
                return;
            }
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
                        if (message.DestinationCharacterId != null)
                        {
                            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                            {
                                DestinationCharacterId = message.SourceCharacterId,
                                SourceCharacterId = message.DestinationCharacterId.Value,
                                SourceWorldId = WorldId,
                                Message = targetSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_CHAT_BLOCKED"), 10),
                                Type = MessageType.PrivateChat
                            });
                        }
                    }
                    else if (targetSession.Character.WhisperBlocked)
                    {
                        if (message.DestinationCharacterId != null)
                        {
                            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                            {
                                DestinationCharacterId = message.SourceCharacterId,
                                SourceCharacterId = message.DestinationCharacterId.Value,
                                SourceWorldId = WorldId,
                                Message = UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_WHISPER_BLOCKED"), 0),
                                Type = MessageType.PrivateChat
                            });
                        }
                    }
                    else
                    {
                        if (message.SourceWorldId != WorldId)
                        {
                            if (message.DestinationCharacterId != null)
                            {
                                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                                {
                                    DestinationCharacterId = message.SourceCharacterId,
                                    SourceCharacterId = message.DestinationCharacterId.Value,
                                    SourceWorldId = WorldId,
                                    Message = targetSession.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("MESSAGE_SENT_TO_CHARACTER"), targetSession.Character.Name, ChannelId), 11),
                                    Type = MessageType.PrivateChat
                                });
                            }
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
                                if (!session.HasSelectedCharacter || session.Character.Family == null)
                                {
                                    return;
                                }
                                if (session.Character.Family.FamilyId == message.DestinationCharacterId)
                                {
                                    session.SendPacket($"say 1 0 6 <{Language.Instance.GetMessageFromKey("CHANNEL")}: {CommunicationServiceClient.Instance.GetChannelIdByWorldId(message.SourceWorldId)}>{message.Message}");
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
                            if (!session.HasSelectedCharacter || session.Character.Family == null)
                            {
                                return;
                            }
                            if (session.Character.Family.FamilyId == message.DestinationCharacterId)
                            {
                                session.SendPacket(message.Message);
                            }
                        });
                    }
                    break;
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
            if (sender == null)
            {
                return;
            }
            Tuple<long?, long?> kickedSession = (Tuple<long?, long?>)sender;

            ClientSession targetSession = Sessions.FirstOrDefault(s => (!kickedSession.Item1.HasValue || s.SessionId == kickedSession.Item1.Value)
                                                                       && (!kickedSession.Item1.HasValue || s.Account.AccountId == kickedSession.Item2));

            targetSession?.Disconnect();
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
