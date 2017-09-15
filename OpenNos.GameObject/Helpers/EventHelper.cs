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
using OpenNos.Domain;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Event.ARENA;
using OpenNos.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OpenNos.GameObject.CommandPackets;

namespace OpenNos.GameObject.Helpers
{
    public class EventHelper
    {
        #region Members

        private static EventHelper _instance;

        #endregion

        #region Properties

        public static EventHelper Instance
        {
            get { return _instance ?? (_instance = new EventHelper()); }
        }

        #endregion

        #region Methods

        public int CalculateComboPoint(int n)
        {
            int a = 4;
            int b = 7;
            for (int i = 0; i < n; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        public void GenerateEvent(EventType type, bool useTimer = true)
        {
            if (!ServerManager.Instance.StartedEvents.Contains(type))
            {
                Task.Factory.StartNew(() =>
                {
                    ServerManager.Instance.StartedEvents.Add(type);
                    switch (type)
                    {
                        case EventType.RANKINGREFRESH:
                            ServerManager.Instance.RefreshRanking();
                            break;

                        case EventType.LOD:
                            LOD.GenerateLod();
                            break;

                        case EventType.MINILANDREFRESHEVENT:
                            MinilandRefresh.GenerateMinilandEvent();
                            break;

                        case EventType.INSTANTBATTLE:
                            InstantBattle.GenerateInstantBattle(useTimer);
                            break;
                        case EventType.TALENTARENA:
                            ArenaEvent.GenerateTalentArena();
                            break;
                        case EventType.LODDH:
                            LOD.GenerateLod(35);
                            break;

                        case EventType.ICEBREAKER:
                            IceBreaker.GenerateIceBreaker(useTimer);
                            break;

                        case EventType.ACT4SHIP:
                            Act4Ship.GenerateAct4Ship(FactionType.Angel);
                            Act4Ship.GenerateAct4Ship(FactionType.Demon);
                            break;
                    }
                });
            }
        }

        public TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));
            TimeSpan timeLeftUntilFirstRun = time - now;
            if (timeLeftUntilFirstRun.TotalHours < 0)
            {
                timeLeftUntilFirstRun += new TimeSpan(24, 0, 0);
            }
            return timeLeftUntilFirstRun;
        }

        public void RunEvent(EventContainer evt, ClientSession session = null, MapMonster monster = null)
        {
            if (session != null)
            {
                evt.MapInstance = session.CurrentMapInstance;
                switch (evt.EventActionType)
                {
                    #region EventForUser

                    case EventActionType.NPCDIALOG:
                        session.SendPacket(session.Character.GenerateNpcDialog((int)evt.Parameter));
                        break;

                    case EventActionType.SENDPACKET:
                        session.SendPacket((string)evt.Parameter);
                        break;

                        #endregion
                }
            }
            if (evt.MapInstance == null)
            {
                return;
            }
            switch (evt.EventActionType)
            {
                #region EventForUser

                case EventActionType.NPCDIALOG:
                case EventActionType.SENDPACKET:
                    if (session == null)
                    {
                        evt.MapInstance.Sessions.ToList().ForEach(e =>
                        {
                            RunEvent(evt, e);
                        });
                    }
                    break;

                #endregion

                #region MapInstanceEvent

                case EventActionType.REGISTEREVENT:
                    Tuple<string, List<EventContainer>> even = (Tuple<string, List<EventContainer>>)evt.Parameter;
                    switch (even.Item1)
                    {
                        case "OnCharacterDiscoveringMap":
                            even.Item2.ForEach(s => evt.MapInstance.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(s, new List<long>())));
                            break;

                        case "OnMoveOnMap":
                            evt.MapInstance.OnMoveOnMapEvents.AddRange(even.Item2);
                            break;

                        case "OnMapClean":
                            evt.MapInstance.OnMapClean.AddRange(even.Item2);
                            break;

                        case "OnLockerOpen":
                            even.Item2.ForEach(s=>evt.MapInstance.InstanceBag.UnlockEvents.Add(s));
                            break;
                    }
                    break;
                case EventActionType.REGISTERWAVE:
                    evt.MapInstance.WaveEvents.Add((EventWave)evt.Parameter);
                    break;
                case EventActionType.SETAREAENTRY:
                    ZoneEvent even2 = (ZoneEvent)evt.Parameter;
                    evt.MapInstance.OnAreaEntryEvents.Add(even2);

                    break;
                case EventActionType.REMOVEMONSTERLOCKER:
                    if (evt.MapInstance.InstanceBag.MonsterLocker.Current > 0)
                    {
                        evt.MapInstance.InstanceBag.MonsterLocker.Current--;
                    }
                    if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                    {
                        evt.MapInstance.InstanceBag.UnlockEvents.ToList().ForEach(s => RunEvent(s));
                        evt.MapInstance.InstanceBag.UnlockEvents.Clear();
                    }
                    break;

                case EventActionType.REMOVEBUTTONLOCKER:
                    if (evt.MapInstance.InstanceBag.ButtonLocker.Current > 0)
                    {
                        evt.MapInstance.InstanceBag.ButtonLocker.Current--;
                    }
                    if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                    {
                        evt.MapInstance.InstanceBag.UnlockEvents.ToList().ForEach(s => RunEvent(s));
                        evt.MapInstance.InstanceBag.UnlockEvents.Clear();
                    }
                    break;

                case EventActionType.EFFECT:
                    short evt3 = (short)evt.Parameter;
                    if (monster!=null && (DateTime.Now - monster.LastEffect).TotalSeconds >= 5)
                    {
                        evt.MapInstance.Broadcast(monster.GenerateEff(evt3));
                        monster.ShowEffect();
                    }
                    break;

                case EventActionType.CONTROLEMONSTERINRANGE:
                    if (monster != null)
                    {
                        Tuple<short, byte, List<EventContainer>> evnt = (Tuple<short, byte, List<EventContainer>>)evt.Parameter;
                        List<MapMonster> mapMonsters = evt.MapInstance.GetListMonsterInRange(monster.MapX, monster.MapY, evnt.Item2);
                        if (evnt.Item1 != 0)
                        {
                            mapMonsters.RemoveAll(s => s.MonsterVNum != evnt.Item1);
                        }
                        mapMonsters.ForEach(s => evnt.Item3.ForEach(e => RunEvent(e, monster: s)));
                    }
                    break;

                case EventActionType.ONTARGET:
                    if (monster?.MoveEvent != null && monster.MoveEvent.InZone(monster.MapX, monster.MapY))
                    {
                        ((List<EventContainer>)evt.Parameter).ForEach(s => RunEvent(s, monster: monster));
                    }
                    break;

                case EventActionType.MOVE:
                    ZoneEvent evt4 = (ZoneEvent)evt.Parameter;
                    if (monster != null)
                    {

                        monster.FirstX = evt4.X;
                        monster.FirstY = evt4.Y;
                        monster.MoveEvent = evt4;
                        monster.Path = BestFirstSearch.FindPath(new Node { X = monster.MapX, Y = monster.MapY }, new Node { X = evt4.X, Y = evt4.Y }, evt.MapInstance?.Map.Grid);
                    }
                    break;

                case EventActionType.CLOCK:
                    evt.MapInstance.InstanceBag.Clock.BasesSecondRemaining = Convert.ToInt32(evt.Parameter);
                    evt.MapInstance.InstanceBag.Clock.DeciSecondRemaining = Convert.ToInt32(evt.Parameter);
                    break;

                case EventActionType.SETMONSTERLOCKERS:
                    evt.MapInstance.InstanceBag.MonsterLocker.Current = Convert.ToByte(evt.Parameter);
                    evt.MapInstance.InstanceBag.MonsterLocker.Initial = Convert.ToByte(evt.Parameter);
                    break;

                case EventActionType.SETBUTTONLOCKERS:
                    evt.MapInstance.InstanceBag.ButtonLocker.Current = Convert.ToByte(evt.Parameter);
                    evt.MapInstance.InstanceBag.ButtonLocker.Initial = Convert.ToByte(evt.Parameter);
                    break;
                case EventActionType.SCRIPTEND:
                    switch (evt.MapInstance.MapInstanceType)
                    {
                        case MapInstanceType.TimeSpaceInstance:
                            evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                            ClientSession client = evt.MapInstance.Sessions.FirstOrDefault();
                            if (client != null)
                            {
                                Guid mapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(client.Character.MapId);
                                MapInstance map = ServerManager.Instance.GetMapInstance(mapInstanceId);
                                ScriptedInstance si = map.ScriptedInstances.FirstOrDefault(s => s.PositionX == client.Character.MapX && s.PositionY == client.Character.MapY);
                                byte penalty = 0;
                                if (si != null && penalty > (client.Character.Level - si.LevelMinimum) * 2)
                                {
                                    penalty = penalty > 100 ? (byte)100 : penalty;
                                    client.SendPacket(client.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("TS_PENALTY"), penalty), 10));
                                }
                                int point = evt.MapInstance.InstanceBag.Point * (100 - penalty) / 100;
                                string perfection = string.Empty;
                                perfection += si != null && evt.MapInstance.InstanceBag.MonstersKilled >= si.MonsterAmount ? 1 : 0;
                                perfection += evt.MapInstance.InstanceBag.NpcsKilled == 0 ? 1 : 0;
                                perfection += si != null && evt.MapInstance.InstanceBag.RoomsVisited >= si.RoomAmount ? 1 : 0;

                                if (si != null)
                                {
                                    evt.MapInstance.Broadcast(
                                        $"score  {evt.MapInstance.InstanceBag.EndState} {point} 27 47 18 {si.DrawItems.Count()} {evt.MapInstance.InstanceBag.MonstersKilled} {si.NpcAmount - evt.MapInstance.InstanceBag.NpcsKilled} {evt.MapInstance.InstanceBag.RoomsVisited} {perfection} 1 1");
                                }
                            }
                            break;
                        case MapInstanceType.RaidInstance:
                            evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                            client = evt.MapInstance.Sessions.FirstOrDefault();
                            List<ClientSession> toBan = new List<ClientSession>();
                            if (client != null)
                            {
                                Group grp = client.Character?.Group;
                                if (grp == null)
                                {
                                    return;
                                }
                                if (evt.MapInstance.InstanceBag.EndState == 1 && evt.MapInstance.Monsters.Any(s => s.IsBoss && !s.IsAlive))
                                {
                                    foreach (ClientSession sess in grp.Characters.Where(s => s.CurrentMapInstance.Monsters.Any(e => e.IsBoss)))
                                    {
                                        // TODO REMOTE THAT FOR PUBLIC RELEASE
                                        if (grp.Characters.Count(s => s.IpAddress.Equals(sess.IpAddress)) > 2)
                                        {
                                            toBan.Add(sess);
                                            continue;
                                        }
                                        if (grp.Raid?.GiftItems == null)
                                        {
                                            continue;
                                        }
                                        if (grp.Raid.Reputation > 0)
                                        {
                                            sess.Character.GetReput(grp.Raid.Reputation);
                                        }
                                        if (sess.Character.Dignity < 0)
                                        {
                                            sess.Character.Dignity += 100;
                                        }
                                        else
                                        {
                                            sess.Character.Dignity = 100;
                                        }
                                        if (sess.Character.Level > grp.Raid.LevelMaximum)
                                        {
                                            // RAID CERTIFICATE
                                            sess.Character.GiftAdd(2320, 1);
                                        }
                                        else
                                        {
                                            foreach (Gift gift in grp.Raid?.GiftItems)
                                            {
                                                sbyte rare = 0;
                                                if (gift.IsRandomRare)
                                                {
                                                    rare = (sbyte) ServerManager.Instance.RandomNumber(-2, 7);
                                                }
                                                //TODO add random rarity for some object
                                                sess.Character.GiftAdd(gift.VNum, gift.Amount, rare, gift.Design);
                                            }
                                        }
                                    }
                                    // Remove monster when raid is over
                                    evt.MapInstance.Monsters.ForEach(e => evt.MapInstance.DespawnMonster(e.MonsterVNum));
                                    evt.MapInstance.WaveEvents.Clear();

                                    ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                                        string.Format(Language.Instance.GetMessageFromKey("RAID_SUCCEED"), grp.Raid?.Label, grp.Characters.ElementAt(0).Character.Name), 0));

                                    Observable.Timer(TimeSpan.FromSeconds(evt.MapInstance.InstanceBag.EndState == 1 ? 30 : 0)).Subscribe(obj =>
                                    {
                                        ClientSession[] grpmembers = new ClientSession[40];
                                        grp.Characters.ToList().CopyTo(grpmembers);
                                        List<MapInstance> mapinstances = new List<MapInstance>();
                                        foreach (ClientSession targetSession in grpmembers)
                                        {
                                            if (targetSession == null)
                                            {
                                                continue;
                                            }
                                            if (targetSession.Character.Hp <= 0)
                                            {
                                                targetSession.Character.Hp = 1;
                                                targetSession.Character.Mp = 1;
                                            }
                                            targetSession.SendPacket(targetSession.Character.GenerateRaidBf(evt.MapInstance.InstanceBag.EndState));
                                            targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                                            targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                                            if (!mapinstances.Any(s => s.MapInstanceId == targetSession?.CurrentMapInstance.MapInstanceId && s.MapInstanceType == MapInstanceType.RaidInstance))
                                            {
                                                mapinstances.Add(targetSession.CurrentMapInstance);
                                            }
                                            grp.LeaveGroup(targetSession);
                                        }
                                        ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                                        ServerManager.Instance.GroupsThreadSafe.TryRemove(grp.GroupId, out Group _);
                                        mapinstances.ForEach(s => s.Dispose());
                                    });
                                }
                            }
                            break;
                    }
                    break;

                case EventActionType.MAPCLOCK:
                    evt.MapInstance.Clock.BasesSecondRemaining = Convert.ToInt32(evt.Parameter);
                    evt.MapInstance.Clock.DeciSecondRemaining = Convert.ToInt32(evt.Parameter);
                    break;

                case EventActionType.STARTCLOCK:
                    Tuple<List<EventContainer>, List<EventContainer>> eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                    if (eve != null)
                    {
                        evt.MapInstance.InstanceBag.Clock.StopEvents = eve.Item2;
                        evt.MapInstance.InstanceBag.Clock.TimeoutEvents = eve.Item1;
                        evt.MapInstance.InstanceBag.Clock.StartClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                    }
                    break;

                case EventActionType.TELEPORT:
                    Tuple<short, short, short, short> tp = (Tuple<short, short, short, short>)evt.Parameter;
                    List<Character> characters = evt.MapInstance.GetCharactersInRange(tp.Item1, tp.Item2, 5).ToList();
                    characters.ForEach(s =>
                    {
                        s.PositionX = tp.Item3;
                        s.PositionY = tp.Item4;
                        evt.MapInstance?.Broadcast(s.Session, s.GenerateTp(), ReceiverType.Group);
                    });
                    break;

                case EventActionType.STOPCLOCK:
                    evt.MapInstance.InstanceBag.Clock.StopClock();
                    evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                    break;

                case EventActionType.STARTMAPCLOCK:
                    eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                    if (eve != null)
                    {
                        evt.MapInstance.Clock.StopEvents = eve.Item2;
                        evt.MapInstance.Clock.TimeoutEvents = eve.Item1;
                        evt.MapInstance.Clock.StartClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                    }
                    break;

                case EventActionType.STOPMAPCLOCK:
                    evt.MapInstance.Clock.StopClock();
                    evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                    break;

                case EventActionType.SPAWNPORTAL:
                    evt.MapInstance.CreatePortal((Portal)evt.Parameter);
                    break;

                case EventActionType.REFRESHMAPITEMS:
                    evt.MapInstance.MapClear();
                    break;

                case EventActionType.NPCSEFFECTCHANGESTATE:
                    evt.MapInstance.Npcs.ForEach(s => s.EffectActivated = (bool)evt.Parameter);
                    break;

                case EventActionType.CHANGEPORTALTYPE:
                    Tuple<int, PortalType> param = (Tuple<int, PortalType>)evt.Parameter;
                    Portal portal = evt.MapInstance.Portals.FirstOrDefault(s => s.PortalId == param.Item1);
                    if (portal != null)
                    {
                        portal.Type = (short)param.Item2;
                    }
                    break;

                case EventActionType.CHANGEDROPRATE:
                    evt.MapInstance.DropRate = (int)evt.Parameter;
                    break;

                case EventActionType.CHANGEXPRATE:
                    evt.MapInstance.XpRate = (int)evt.Parameter;
                    break;

                case EventActionType.DISPOSEMAP:
                    evt.MapInstance.Dispose();
                    break;

                case EventActionType.SPAWNBUTTON:
                    evt.MapInstance.SpawnButton((MapButton)evt.Parameter);
                    break;

                case EventActionType.UNSPAWNMONSTERS:
                    evt.MapInstance.DespawnMonster((int)evt.Parameter);
                    break;

                case EventActionType.SPAWNMONSTERS:
                    evt.MapInstance.SummonMonsters((List<MonsterToSummon>)evt.Parameter);
                    break;
                case EventActionType.REFRESHRAIDGOAL:
                    ClientSession cl = evt.MapInstance.Sessions.FirstOrDefault();
                    if (cl?.Character != null)
                    {
                        ServerManager.Instance.Broadcast(cl, cl.Character?.Group?.GeneraterRaidmbf(), ReceiverType.Group);
                        ServerManager.Instance.Broadcast(cl, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NEW_MISSION"), 0), ReceiverType.Group);
                    }
                    break;
                case EventActionType.SPAWNNPCS:
                    evt.MapInstance.SummonNpcs((List<NpcToSummon>)evt.Parameter);
                    break;

                case EventActionType.DROPITEMS:
                    evt.MapInstance.DropItems((List<Tuple<short, int, short, short>>)evt.Parameter);
                    break;

                case EventActionType.THROWITEMS:
                    Tuple<int, short, byte, int, int> parameters = (Tuple<int, short, byte, int, int>)evt.Parameter;
                    if (monster != null)
                    {
                        parameters = new Tuple<int, short, byte, int, int>(monster.MapMonsterId, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5);
                    }
                    evt.MapInstance.ThrowItems(parameters);
                    break;

                case EventActionType.SPAWNONLASTENTRY:
                    Character lastincharacter = evt.MapInstance.Sessions.OrderByDescending(s => s.RegisterTime).FirstOrDefault()?.Character;
                    List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                    MapCell hornSpawn = new MapCell
                    {
                        X = lastincharacter?.PositionX ?? 154,
                        Y = lastincharacter?.PositionY ?? 140
                    };
                    long hornTarget = lastincharacter?.CharacterId ?? -1;
                    summonParameters.Add(new MonsterToSummon(Convert.ToInt16(evt.Parameter), hornSpawn, hornTarget, true));
                    evt.MapInstance.SummonMonsters(summonParameters);
                    break;

                #endregion
            }
        }

        public void ScheduleEvent(TimeSpan timeSpan, EventContainer evt)
        {
            Observable.Timer(timeSpan).Subscribe(x =>
            {
                RunEvent(evt);
            });
        }

        #endregion
    }
}