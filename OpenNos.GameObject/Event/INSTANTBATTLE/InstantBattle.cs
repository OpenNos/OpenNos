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
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace OpenNos.GameObject.Event
{
    public class InstantBattle
    {
        #region Methods

        public static void GenerateInstantBattle()
        {
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 0));
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 1));
            Thread.Sleep(4 * 60 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 0));
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 1));
            Thread.Sleep(30 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 0));
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 1));
            Thread.Sleep(20 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 0));
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 1));
            Thread.Sleep(10 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"), 1));
            ServerManager.Instance.Broadcast($"qnaml 1 #guri^506 {Language.Instance.GetMessageFromKey("INSTANTBATTLE_QUESTION")}");
            ServerManager.Instance.EventInWaiting = true;
            Thread.Sleep(30 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"), 1));
            ServerManager.Instance.Sessions.Where(s => s.Character != null && !s.Character.IsWaitingForEvent).ToList().ForEach(s => s.SendPacket("esf"));
            ServerManager.Instance.EventInWaiting = false;
            IEnumerable<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.IsWaitingForEvent && s.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance);
            List<Tuple<MapInstance, byte>> maps = new List<Tuple<MapInstance, byte>>();
            MapInstance map = null;
            int i = -1;
            int level = 0;
            byte instancelevel = 1;
            foreach (ClientSession s in sessions.OrderBy(s => s.Character?.Level))
            {
                i++;
                if (s.Character.Level > 79 && level <= 79)
                {
                    i = 0;
                    instancelevel = 80;
                }
                else if (s.Character.Level > 69 && level <= 69)
                {
                    i = 0;
                    instancelevel = 70;
                }
                else if (s.Character.Level > 59 && level <= 59)
                {
                    i = 0;
                    instancelevel = 60;
                }
                else if (s.Character.Level > 49 && level <= 49)
                {
                    i = 0;
                    instancelevel = 50;
                }
                else if (s.Character.Level > 39 && level <= 39)
                {
                    i = 0;
                    instancelevel = 30;
                }
                if (i % 50 == 0)
                {
                    map = ServerManager.Instance.GenerateMapInstance(2004, MapInstanceType.NormalInstance, new InstanceBag());
                    maps.Add(new Tuple<MapInstance, byte>(map, instancelevel));
                }
                if (map != null)
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(s, map.MapInstanceId);
                }

                level = s.Character.Level;
            }
            ServerManager.Instance.Sessions.Where(s => s.Character != null).ToList().ForEach(s => s.Character.IsWaitingForEvent = false);
            long maxGold = ServerManager.Instance.MaxGold;
            foreach (Tuple<MapInstance, byte> mapinstance in maps)
            {
                ServerManager.Instance.StartedEvents.Remove(EventType.INSTANTBATTLE);
                Thread.Sleep(10 * 1000);
                if (mapinstance.Item1.Sessions.Count() < 3)
                {
                    mapinstance.Item1.Sessions.Where(s => s.Character != null).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                }
                Observable.Timer(TimeSpan.FromMinutes(12)).Subscribe(X =>
                {
                    for (int d = 0; d < 180; d++)
                    {
                        if (!mapinstance.Item1.Monsters.Any(s => s.CurrentHp > 0))
                        {
                            EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0), new EventContainer(mapinstance.Item1, EventActionType.SPAWNPORTAL, new Portal { SourceX = 47, SourceY = 33, DestinationMapId = 1 }));
                            mapinstance.Item1.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SUCCEEDED"), 0));
                            foreach (ClientSession cli in mapinstance.Item1.Sessions.Where(s => s.Character != null).ToList())
                            {
                                cli.Character.GetReput(cli.Character.Level * 50);
                                cli.Character.Gold += cli.Character.Level * 1000;
                                cli.Character.Gold = cli.Character.Gold > maxGold ? maxGold : cli.Character.Gold;
                                cli.Character.SpAdditionPoint += cli.Character.Level * 100;
                                cli.Character.SpAdditionPoint = cli.Character.SpAdditionPoint > 1000000 ? 1000000 : cli.Character.SpAdditionPoint;
                                cli.SendPacket(cli.Character.GenerateSpPoint());
                                cli.SendPacket(cli.Character.GenerateGold());
                                cli.SendPacket(cli.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_MONEY"), cli.Character.Level * 1000), 10));
                                cli.SendPacket(cli.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_REPUT"), cli.Character.Level * 50), 10));
                                cli.SendPacket(cli.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_SP_POINT"), cli.Character.Level * 100), 10));
                            }
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                });

                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(15), new EventContainer(mapinstance.Item1, EventActionType.DISPOSEMAP, null));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(3), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 12), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(5), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 10), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(10), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 5), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(11), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 4), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(12), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 3), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(13), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 2), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 1), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(7), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_APPEAR"), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(3), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));

                for (int wave = 0; wave < 4; wave++)
                {
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(130 + wave * 160), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WAVE"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(160 + wave * 160), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(170 + wave * 160), new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10 + wave * 160), new EventContainer(mapinstance.Item1, EventActionType.SPAWNMONSTERS, GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, wave)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(140 + wave * 160), new EventContainer(mapinstance.Item1, EventActionType.DROPITEMS, GetInstantBattleDrop(mapinstance.Item1.Map, mapinstance.Item2, wave)));
                }
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(650), new EventContainer(mapinstance.Item1, EventActionType.SPAWNMONSTERS, GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, 4)));
            }
        }

        private static IEnumerable<Tuple<short, int, short, short>> GenerateDrop(Map map, short vnum, int amountofdrop, int amount)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            for (int i = 0; i < amountofdrop; i++)
            {
                MapCell cell = map.GetRandomPosition();
                dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
            }
            return dropParameters;
        }

        private static List<Tuple<short, int, short, short>> GetInstantBattleDrop(Map map, short instantbattletype, int wave)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            switch (instantbattletype)
            {
                case 1:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 500));
                            dropParameters.AddRange(GenerateDrop(map, 2027, 8, 5));
                            dropParameters.AddRange(GenerateDrop(map, 2018, 5, 5));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1000));
                            dropParameters.AddRange(GenerateDrop(map, 1002, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1005, 16, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1500));
                            dropParameters.AddRange(GenerateDrop(map, 1002, 10, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1005, 10, 5));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2000));
                            dropParameters.AddRange(GenerateDrop(map, 1003, 10, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1006, 10, 5));
                            break;
                    }
                    break;

                case 40:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1500));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 5, 3));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2000));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2500));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 5, 1));
                            break;
                    }
                    break;

                case 50:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 1500));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 5, 3));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2000));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 2500));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 5, 1));
                            break;
                    }
                    break;

                case 60:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 8, 4));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 4000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 10, 13));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 8, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 8, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 7000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1029, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 13, 1));
                            break;
                    }
                    break;

                case 70:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 3000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 4000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 15, 4));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 10, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 5000));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 13, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 13, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 7000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 13, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1029, 5, 1));
                            break;
                    }
                    break;

                case 80:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 10000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 15, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 15, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 12000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 15, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 15, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15, 15000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 20, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 15, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 15, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 30, 20000));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 30, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1030, 30, 1));
                            dropParameters.AddRange(GenerateDrop(map, 2282, 12, 3));
                            break;
                    }
                    break;
            }
            return dropParameters;
        }

        private static List<MonsterToSummon> GetInstantBattleMonster(Map map, short instantbattletype, int wave)
        {
            List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();

            switch (instantbattletype)
            {
                case 1:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(map.GenerateMonsters(1, 16, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(58, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(105, 16, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(107, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(108, 8, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(111, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(136, 15, true, new List<EventContainer>()));
                            break;

                        case 1:
                            SummonParameters.AddRange(map.GenerateMonsters(194, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(114, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(99, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(39, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(2, 16, true, new List<EventContainer>()));
                            break;

                        case 2:
                            SummonParameters.AddRange(map.GenerateMonsters(140, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(100, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(81, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(12, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(4, 16, true, new List<EventContainer>()));
                            break;

                        case 3:
                            SummonParameters.AddRange(map.GenerateMonsters(115, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(112, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(110, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(14, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(5, 16, true, new List<EventContainer>()));
                            break;

                        case 4:
                            SummonParameters.AddRange(map.GenerateMonsters(979, 1, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(167, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(137, 10, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(22, 15, false, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(17, 8, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(16, 16, true, new List<EventContainer>()));
                            break;
                    }
                    break;

                case 40:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(map.GenerateMonsters(120, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(151, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(149, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(139, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(73, 16, true, new List<EventContainer>()));
                            break;

                        case 1:
                            SummonParameters.AddRange(map.GenerateMonsters(152, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(147, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(104, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(62, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(8, 16, true, new List<EventContainer>()));
                            break;

                        case 2:
                            SummonParameters.AddRange(map.GenerateMonsters(153, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(132, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(86, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(76, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(68, 16, true, new List<EventContainer>()));
                            break;

                        case 3:
                            SummonParameters.AddRange(map.GenerateMonsters(134, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(91, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(133, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(70, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(89, 16, true, new List<EventContainer>()));
                            break;

                        case 4:
                            SummonParameters.AddRange(map.GenerateMonsters(154, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(200, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(77, 8, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(217, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(724, 1, true, new List<EventContainer>()));
                            break;
                    }
                    break;

                case 50:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(map.GenerateMonsters(134, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(91, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(89, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(77, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(71, 16, true, new List<EventContainer>()));
                            break;

                        case 1:
                            SummonParameters.AddRange(map.GenerateMonsters(217, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(200, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(154, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(92, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(79, 16, true, new List<EventContainer>()));
                            break;

                        case 2:
                            SummonParameters.AddRange(map.GenerateMonsters(235, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(226, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(214, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(204, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(201, 15, true, new List<EventContainer>()));
                            break;

                        case 3:
                            SummonParameters.AddRange(map.GenerateMonsters(249, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(236, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(227, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(218, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(202, 15, true, new List<EventContainer>()));
                            break;

                        case 4:
                            SummonParameters.AddRange(map.GenerateMonsters(583, 1, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(400, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(255, 8, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(253, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(251, 10, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(205, 14, true, new List<EventContainer>()));
                            break;
                    }
                    break;

                case 60:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(map.GenerateMonsters(242, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(234, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(215, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(207, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(202, 13, true, new List<EventContainer>()));
                            break;

                        case 1:
                            SummonParameters.AddRange(map.GenerateMonsters(402, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(253, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(237, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(216, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(205, 13, true, new List<EventContainer>()));
                            break;

                        case 2:
                            SummonParameters.AddRange(map.GenerateMonsters(402, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(243, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(228, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(255, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(205, 13, true, new List<EventContainer>()));
                            break;

                        case 3:
                            SummonParameters.AddRange(map.GenerateMonsters(268, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(255, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(254, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(174, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(172, 13, true, new List<EventContainer>()));
                            break;

                        case 4:
                            SummonParameters.AddRange(map.GenerateMonsters(725, 1, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(407, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(272, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(261, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(256, 12, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(275, 13, true, new List<EventContainer>()));
                            break;
                    }
                    break;

                case 70:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(map.GenerateMonsters(402, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(253, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(237, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(216, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(205, 15, true, new List<EventContainer>()));
                            break;

                        case 1:
                            SummonParameters.AddRange(map.GenerateMonsters(402, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(243, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(228, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(225, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(205, 15, true, new List<EventContainer>()));
                            break;

                        case 2:
                            SummonParameters.AddRange(map.GenerateMonsters(255, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(254, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(251, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(174, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(172, 15, true, new List<EventContainer>()));
                            break;

                        case 3:
                            SummonParameters.AddRange(map.GenerateMonsters(407, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(272, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(261, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(257, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(256, 15, true, new List<EventContainer>()));
                            break;

                        case 4:
                            SummonParameters.AddRange(map.GenerateMonsters(748, 1, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(444, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(439, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(275, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(274, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(273, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(163, 13, true, new List<EventContainer>()));
                            break;
                    }
                    break;

                case 80:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(map.GenerateMonsters(1007, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1003, 15, false, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1002, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1001, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1000, 16, true, new List<EventContainer>()));
                            break;

                        case 1:
                            SummonParameters.AddRange(map.GenerateMonsters(1199, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1198, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1197, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1196, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1123, 16, true, new List<EventContainer>()));
                            break;

                        case 2:
                            SummonParameters.AddRange(map.GenerateMonsters(1305, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1304, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1303, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1302, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1194, 16, true, new List<EventContainer>()));
                            break;

                        case 3:
                            SummonParameters.AddRange(map.GenerateMonsters(1902, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1901, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1900, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1045, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1043, 15, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1042, 16, true, new List<EventContainer>()));
                            break;

                        case 4:
                            SummonParameters.AddRange(map.GenerateMonsters(637, 1, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1903, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1053, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1051, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1049, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1048, 13, true, new List<EventContainer>()));
                            SummonParameters.AddRange(map.GenerateMonsters(1047, 13, true, new List<EventContainer>()));
                            break;
                    }
                    break;
            }
            return SummonParameters;
        }

        #endregion
    }
}