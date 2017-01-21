using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.WebApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class EventHelper
    {
        static void GenerateInstantBattle()
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
            IEnumerable<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.IsWaitingForEvent);
            List<MapInstance> maps = new List<MapInstance>();
            MapInstance map = null;
            int i = 0;
            foreach (ClientSession s in sessions)
            {
                if (i % 50 == 0)
                {
                    map = ServerManager.GenerateMapInstance(2004, MapInstanceType.NormalInstance);
                    maps.Add(map);
                }
                ServerManager.Instance.TeleportOnRandomPlaceInMap(s, map.MapInstanceId);
                i++;
            }
            foreach (MapInstance mapinstance in maps)
            {
                Observable.Timer(TimeSpan.FromMinutes(12)).Subscribe(X =>
                {
                    for (int d = 0; d < 180; d++)
                    {
                        if (!mapinstance.Monsters.Any(s => s.CurrentHp > 0))
                        {
                            mapinstance.CreatePortal(new Portal() { SourceX = 47, SourceY = 33, DestinationMapId = 1 });
                            mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SUCCEEDED"), 0));
                            foreach (ClientSession cli in mapinstance.Sessions.Where(s => s.Character != null).ToList())
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



                Observable.Timer(TimeSpan.FromMinutes(15)).Subscribe(X => { mapinstance.Dispose(); ServerManager.Instance.StartedEvents.Remove(EventType.INSTANTBATTLE); });
                Observable.Timer(TimeSpan.FromMinutes(3)).Subscribe(x => { mapinstance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 12), 0)); });
                Observable.Timer(TimeSpan.FromMinutes(5)).Subscribe(x => { mapinstance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 10), 0)); });
                Observable.Timer(TimeSpan.FromMinutes(10)).Subscribe(x =>
                {
                    for (int g = 5; g > 0; g--)
                    {
                        mapinstance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), g), 0));
                    }
                });
                Observable.Timer(TimeSpan.FromSeconds(60 * 14 + 30)).Subscribe(x => { mapinstance.Broadcast(ServerManager.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30), 0)); });
                Thread.Sleep(10 * 1000);

                if (mapinstance.Sessions.Count() < 3)
                {
                    mapinstance.Sessions.Where(s => s.Character != null).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.PositionX, s.Character.PositionY));
                }

                mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WALKS"), 0));
                Thread.Sleep(7 * 1000);
                mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_APPEAR"), 0));
                Thread.Sleep(3 * 1000);
                mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0));


                Observable.Timer(TimeSpan.FromSeconds(0)).Subscribe(X => { for (int wave = 0; wave < 4; wave++) { mapinstance.SummonMonsters(InstantBattleHelper.GetInstantBattleMonster(mapinstance.Map, 70, wave)); Thread.Sleep(160 * 1000); } });
                Observable.Timer(TimeSpan.FromSeconds(120)).Subscribe(X => { for (int wave = 0; wave < 4; wave++) { mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WAVE"), 0)); Thread.Sleep(160 * 1000); } });
                Observable.Timer(TimeSpan.FromSeconds(130)).Subscribe(X => { for (int wave = 0; wave < 4; wave++) { mapinstance.DropItems(InstantBattleHelper.GetInstantBattleDrop(mapinstance.Map, 70, wave)); Thread.Sleep(160 * 1000); } });
                Observable.Timer(TimeSpan.FromSeconds(150)).Subscribe(X => { for (int wave = 0; wave < 4; wave++) { mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WALKS"), 0)); Thread.Sleep(160 * 1000); } });
                Observable.Timer(TimeSpan.FromSeconds(160)).Subscribe(X => { for (int wave = 0; wave < 4; wave++) { mapinstance.Broadcast(ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)); Thread.Sleep(160 * 1000); } });

            }


        }

        static void GenerateReput()
        {
            foreach (var genlog in ServerManager.GeneralLogs.Where(s => s.LogData == "MINILAND" && s.Timestamp > DateTime.Now.AddDays(-1)).GroupBy(s => s.CharacterId))
            {
                ClientSession Session = ServerManager.Instance.GetSessionByCharacterId((long)genlog.Key);
                if (Session == null)
                {
                    Session.Character.GetReput(2 * genlog.Count());
                }
                else if (!ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("CharacterIsConnected", (long)genlog.Key).Result)
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadById((long)genlog.Key);
                    if (chara != null)
                    {
                        chara.Reput += 2 * genlog.Count();
                        DAOFactory.CharacterDAO.InsertOrUpdate(ref chara);
                    }
                }
                ServerManager.Instance.StartedEvents.Remove(EventType.REPUTEVENT);
            }
        }

        static void GenerateLod()
        {
            int lodtime = 120;
            int HornTime = 30;
            int HornRepawn = 4;
            int HornStay = 1;
            ServerManager.Instance.EnableMapEffect(98, true);
            Dictionary<long, IDisposable> dict = new Dictionary<long, IDisposable>();
            foreach (Family fam in ServerManager.Instance.FamilyList)
            {

                MapInstance LandOfDeath = ServerManager.GenerateMapInstance(150, MapInstanceType.LodInstance);
                LandOfDeath.StartClock((int)(TimeSpan.FromMinutes(lodtime).TotalSeconds * 10));
                Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime)).Subscribe(x => { LandOfDeath.XpRate = 3; LandOfDeath.DropRate = 3; });
                IDisposable disp = Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime)).Subscribe(
                      x =>
                      {
                          for (int i = 0; i < 10; i++)
                          {
                              Character lastincharacter = LandOfDeath.GetLastInCharacter();
                              List<Tuple<short, short, short, long, bool>> SummonParameters = new List<Tuple<short, short, short, long, bool>>();
                              SummonParameters.Add(new Tuple<short, short, short, long, bool>(443, (lastincharacter != null ? lastincharacter.PositionX : (short)154), (lastincharacter != null ? lastincharacter.PositionY : (short)140), lastincharacter != null ? lastincharacter.CharacterId : -1, true));
                              LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket("df 2"));
                              LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0)));
                              List<int> monsterIds = LandOfDeath.SummonMonsters(SummonParameters);
                              Observable.Timer(TimeSpan.FromMinutes(HornStay)).Subscribe(c =>
                              {
                                  LandOfDeath.Lock = true;
                                  LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0)));
                                  LandOfDeath.UnspawnMonsters(monsterIds);
                              });
                              Thread.Sleep(HornRepawn * 1000);
                          }
                      });
                Observable.Timer(TimeSpan.FromMinutes(lodtime)).Subscribe(x => { disp.Dispose(); LandOfDeath.Dispose(); ServerManager.Instance.StartedEvents.Remove(EventType.LOD); ServerManager.Instance.EnableMapEffect(98, false); });
                fam.LandOfDeath = LandOfDeath;
            }


        }

        public static TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));
            TimeSpan timeLeftUntilFirstRun = time - now;
            if (timeLeftUntilFirstRun.TotalHours < 0)
                timeLeftUntilFirstRun += new TimeSpan(24, 0, 0);
            return timeLeftUntilFirstRun;
        }

        public static void GenerateEvent(EventType type)
        {
            if (!ServerManager.Instance.StartedEvents.Contains(type))
            {
                Task.Factory.StartNew(() =>
                {
                    ServerManager.Instance.StartedEvents.Add(type);
                    switch (type)
                    {
                        case EventType.LOD:
                            GenerateLod();
                            break;
                        case EventType.REPUTEVENT:
                            GenerateReput();
                            break;
                        case EventType.INSTANTBATTLE:
                            GenerateInstantBattle();
                            break;
                    }
                });
            }
        }
    }


}
