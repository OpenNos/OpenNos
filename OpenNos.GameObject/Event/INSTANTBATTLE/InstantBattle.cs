using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class InstantBattle
    {
      public static void GenerateInstantBattle()
        {
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_MINUTES"), 5), 0));
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_MINUTES"), 5), 1));
            Thread.Sleep(4 * 60 * 1000);
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_MINUTES"), 1), 0));
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_MINUTES"), 1), 1));
            Thread.Sleep(30 * 1000);
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_SECONDS"), 30), 0));
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_SECONDS"), 30), 1));
            Thread.Sleep(20 * 1000);
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_SECONDS"), 10), 0));
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("ISTANTBATTLE_SECONDS"), 10), 1));
            Thread.Sleep(10 * 1000);
            ServerManager.Instance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("ISTANTBATTLE_STARTED"), 1));
            ServerManager.Instance.Broadcast($"qnaml 1 #guri^506 {Language.Instance.GetMessageFromKey("ISTANTBATTLE_QUESTION")}");
            ServerManager.Instance.EventInWaiting = true;
            Thread.Sleep(30 * 1000);
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
                    map = ServerManager.GenerateMapInstance(2004, MapInstanceType.NormalInstance);
                    maps.Add(new Tuple<MapInstance, byte>(map, instancelevel));
                }
                ServerManager.Instance.TeleportOnRandomPlaceInMap(s, map.MapInstanceId);

                level = s.Character.Level;
            }
            ServerManager.Instance.Sessions.Where(s => s.Character != null).ToList().ForEach(s => s.Character.IsWaitingForEvent = false);
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
                            mapinstance.Item1.CreatePortal(new Portal() { SourceX = 47, SourceY = 33, DestinationMapId = 1 });
                            mapinstance.Item1.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SUCCEEDED"), 0));
                            foreach (ClientSession cli in mapinstance.Item1.Sessions.Where(s => s.Character != null).ToList())
                            {
                                cli.Character.GetReput(cli.Character.Level * 50);
                                cli.Character.Gold += cli.Character.Level * 1000;
                                cli.Character.Gold = (cli.Character.Gold > 1000000000) ? 1000000000 : cli.Character.Gold;
                                cli.Character.SpAdditionPoint += cli.Character.Level * 100;
                                cli.Character.SpAdditionPoint = (cli.Character.SpAdditionPoint > 1000000) ? 1000000 : cli.Character.SpAdditionPoint;
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

                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(15), EventActionType.DISPOSE, null);
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(3), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 12), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(5), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 10), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(10), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 5), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(11), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 4), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(12), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 3), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(13), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 2), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(14), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 1), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(14.5), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(14.5), EventActionType.MESSAGE, ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(0), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WALKS"), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(7), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_APPEAR"), 0));
                mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(3), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0));

                for (int wave = 0; wave < 4; wave++)
                {
                    mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(130 + wave * 160), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WAVE"), 0));
                    mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(160 + wave * 160), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WALKS"), 0));
                    mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(170 + wave * 160), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0));
                    mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(10 + wave * 160), EventActionType.SPAWN, GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, wave));
                    mapinstance.Item1.StartMapEvent(TimeSpan.FromSeconds(140 + wave * 160), EventActionType.DROPITEMS, GetInstantBattleDrop(mapinstance.Item1.Map, mapinstance.Item2, wave));
                }
                mapinstance.Item1.StartMapEvent(TimeSpan.FromMinutes(13.5), EventActionType.SPAWN, GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, 5));
            }

        }




        public static List<MonsterToSummon> GenerateMonsters(Map map, short vnum, short amount, bool move)
        {
            List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();
            MapCell cell;
            for (int i = 0; i < amount; i++)
            {
                cell = map.GetRandomPosition();
                SummonParameters.Add(new MonsterToSummon(vnum, cell, -1, true));
            }
            return SummonParameters;
        }

        public static List<Tuple<short, int, short, short>> GenerateDrop(Map map, short vnum, int amountofdrop, int amount)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            MapCell cell = null;
            for (int i = 0; i < amountofdrop; i++)
            {
                cell = map.GetRandomPosition();
                dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
            }
            return dropParameters;
        }

        public static List<MonsterToSummon> GetInstantBattleMonster(Map map, short instantbattletype, int wave)
        {
            List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();

            switch (instantbattletype)
            {

                case 1:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 1, 16, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 58, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 105, 16, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 107, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 108, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 111, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 136, 15, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 194, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 114, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 99, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 39, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 2, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 140, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 100, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 81, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 12, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 4, 16, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 115, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 112, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 110, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 14, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 5, 16, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 979, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 167, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 137, 10, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 22, 15, false));
                            SummonParameters.AddRange(GenerateMonsters(map, 17, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 16, 16, true));
                            break;
                    }
                    break;
                case 40:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 120, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 151, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 149, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 139, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 73, 16, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 152, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 147, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 104, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 62, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 8, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 153, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 132, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 86, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 76, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 68, 16, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 134, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 91, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 133, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 70, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 89, 16, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 154, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 200, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 77, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 217, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 724, 1, true));
                            break;
                    }
                    break;
                case 50:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 134, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 91, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 89, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 77, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 71, 16, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 217, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 200, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 154, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 92, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 79, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 235, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 226, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 214, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 204, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 201, 15, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 249, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 236, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 227, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 218, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 202, 15, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 583, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 400, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 8, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 253, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 251, 10, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 14, true));
                            break;
                    }
                    break;
                case 60:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 242, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 234, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 215, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 207, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 202, 13, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 253, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 237, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 216, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 13, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 243, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 228, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 13, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 268, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 254, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 174, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 172, 13, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 725, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 407, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 272, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 261, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 256, 12, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 275, 13, true));
                            break;
                    }
                    break;
                case 70:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 253, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 237, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 216, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 15, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 402, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 243, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 228, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 225, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 205, 15, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 255, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 254, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 251, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 174, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 172, 15, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 407, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 272, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 261, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 257, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 256, 15, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 748, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 444, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 439, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 275, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 274, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 273, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 163, 13, true));
                            break;
                    }
                    break;
                case 80:
                    switch (wave)
                    {
                        case 0:
                            SummonParameters.AddRange(GenerateMonsters(map, 1007, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1003, 15, false));
                            SummonParameters.AddRange(GenerateMonsters(map, 1002, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1001, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1000, 16, true));
                            break;
                        case 1:
                            SummonParameters.AddRange(GenerateMonsters(map, 1199, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1198, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1197, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1196, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1123, 16, true));
                            break;
                        case 2:
                            SummonParameters.AddRange(GenerateMonsters(map, 1305, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1304, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1303, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1302, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1194, 16, true));
                            break;
                        case 3:
                            SummonParameters.AddRange(GenerateMonsters(map, 1902, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1901, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1900, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1045, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1043, 15, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1042, 16, true));
                            break;
                        case 4:
                            SummonParameters.AddRange(GenerateMonsters(map, 637, 1, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1903, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1053, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1051, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1049, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1048, 13, true));
                            SummonParameters.AddRange(GenerateMonsters(map, 1047, 13, true));
                            break;
                    }
                    break;
            }
            return SummonParameters;
        }

        public static List<Tuple<short, int, short, short>> GetInstantBattleDrop(Map map, short instantbattletype, int wave)
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
    }
}
