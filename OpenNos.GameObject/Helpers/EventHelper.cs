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

using CloneExtensions;
using OpenNos.Domain;
using OpenNos.GameObject.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Helpers
{
    public class EventHelper
    {
        #region Instantiation
        private static EventHelper instance;

        #endregion

        public static EventHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventHelper();
                }
                return instance;
            }
        }
        #region Methods

        public void GenerateEvent(EventType type)
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
                            InstantBattle.GenerateInstantBattle();
                            break;

                        case EventType.LODDH:
                            LOD.GenerateLod(35);
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

        public void RunEvent(EventContainer evt, ClientSession session = null)
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
            if (evt.MapInstance != null)
            {
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
                        switch(even.Item1)
                        {
                            case "OnCharacterDiscoveringMap":
                                even.Item2.ForEach(s => evt.MapInstance.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer,List<long>>(s,new List<long>())));
                                break;
                            case "OnMoveOnMap":
                                evt.MapInstance.OnMoveOnMapEvents.AddRange(even.Item2);
                                break;
                            case "OnMapClean":
                                evt.MapInstance.OnMapClean.AddRange(even.Item2);
                                break;
                        } 
                        break;
                    case EventActionType.CLOCK:
                        evt.MapInstance.InstanceBag.Clock.BasesSecondRemaining = Convert.ToInt32(evt.Parameter);
                        evt.MapInstance.InstanceBag.Clock.DeciSecondRemaining = Convert.ToInt32(evt.Parameter);
                        break;

                    case EventActionType.MAPCLOCK:
                        evt.MapInstance.Clock.BasesSecondRemaining = Convert.ToInt32(evt.Parameter);
                        evt.MapInstance.Clock.DeciSecondRemaining = Convert.ToInt32(evt.Parameter);
                        break;

                    case EventActionType.STARTCLOCK:
                        Tuple<List<EventContainer>, List<EventContainer>> eve =(Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                        evt.MapInstance.InstanceBag.Clock.StopEvents = eve.Item2;
                        evt.MapInstance.InstanceBag.Clock.TimeoutEvents = eve.Item1;
                        evt.MapInstance.InstanceBag.Clock.StartClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                        break;

                    case EventActionType.STOPCLOCK:
                        evt.MapInstance.InstanceBag.Clock.StopClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                        break;

                    case EventActionType.STARTMAPCLOCK:
                        eve = (Tuple<List<EventContainer>, List<EventContainer>>)evt.Parameter;
                        evt.MapInstance.Clock.StopEvents = eve.Item2;
                        evt.MapInstance.Clock.TimeoutEvents = eve.Item1;
                        evt.MapInstance.Clock.StartClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
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
                        evt.MapInstance.Npcs.GetAllItems().ForEach(s => s.EffectActivated = (bool)evt.Parameter);
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
                        evt.MapInstance.UnspawnMonsters((int)evt.Parameter);
                        break;

                    case EventActionType.SPAWNMONSTERS:
                        evt.MapInstance.SummonMonsters((List<MonsterToSummon>)evt.Parameter);
                        break;

                    case EventActionType.SPAWNNPCS:
                        evt.MapInstance.SummonNpcs((List<NpcToSummon>)evt.Parameter);
                        break;

                    case EventActionType.DROPITEMS:
                        evt.MapInstance.DropItems((List<Tuple<short, int, short, short>>)evt.Parameter);
                        break;

                    case EventActionType.SPAWNONLASTENTRY:

                        //TODO REVIEW THIS CASE
                        Character lastincharacter = evt.MapInstance.Sessions.OrderByDescending(s => s.RegisterTime).FirstOrDefault()?.Character;
                        List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                        MapCell hornSpawn = new MapCell
                        {
                            X = lastincharacter?.PositionX ?? 154,
                            Y = lastincharacter?.PositionY ?? 140
                        };
                        long HornTarget = lastincharacter != null ? lastincharacter.CharacterId : -1;
                        summonParameters.Add(new MonsterToSummon((short)evt.Parameter, hornSpawn, HornTarget, true, new List<EventContainer>()));
                        evt.MapInstance.SummonMonsters(summonParameters);
                        break;

                        #endregion
                }
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