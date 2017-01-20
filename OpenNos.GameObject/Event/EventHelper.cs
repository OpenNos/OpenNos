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
        public static void GenerateReput()
        {
            foreach (Schedule schedul in ServerManager.Schedules.Where(s => s.Event == "REPUTEVENT"))
            {
                Observable.Timer(TimeSpan.FromSeconds(EventHelper.GetMilisecondsBeforeTime(schedul.Time).TotalSeconds), TimeSpan.FromDays(1))
               .Subscribe(
               x =>
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
                   }
               });
            }
        }
        public static void GenerateLod()
        {
            ServerManager.Instance.EnableMapEffect(98, false);
            foreach (Schedule schedul in ServerManager.Schedules.Where(s => s.Event == "LOD"))
            {
                Observable.Timer(TimeSpan.FromSeconds(EventHelper.GetMilisecondsBeforeTime(schedul.Time).TotalSeconds), TimeSpan.FromDays(1))
                .Subscribe(
                e =>
                {
                    ServerManager.Instance.EnableMapEffect(98, true);
                    foreach (Family fam in ServerManager.Instance.FamilyList)
                    {

                        int lodtime = 120;
                        int HornTime = 30;
                        int HornRepawn = 4;
                        int HornStay = 1;
                        MapInstance LandOfDeath = ServerManager.GenerateMapInstance(150, MapInstanceType.LodInstance);
                        LandOfDeath.StartClock((int)(TimeSpan.FromMinutes(lodtime).TotalSeconds * 10));
                        Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime)).Subscribe(x => { LandOfDeath.XpRate = 3; LandOfDeath.DropRate = 3; });
                        Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime), TimeSpan.FromMinutes(HornRepawn)).Subscribe(
                            x =>
                            {
                                Character lastincharacter = LandOfDeath.GetLastInCharacter();
                                List<Tuple<short, short, short, long>> SummonParameters = new List<Tuple<short, short, short, long>>();
                                SummonParameters.Add(new Tuple<short, short, short, long>(443, (lastincharacter != null ? lastincharacter.PositionX : (short)154), (lastincharacter != null ? lastincharacter.PositionY : (short)140), lastincharacter != null ? lastincharacter.CharacterId : -1));
                                LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket("df 2"));
                                LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0)));
                                List<int> monsterIds = LandOfDeath.SummonMonster(SummonParameters);
                                Observable.Timer(TimeSpan.FromMinutes(HornStay)).Subscribe(c =>
                                {
                                    LandOfDeath.Lock = true;
                                    LandOfDeath.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0)));
                                    LandOfDeath.UnspawnMonsters(monsterIds);
                                });
                            });
                        Observable.Timer(TimeSpan.FromMinutes(lodtime)).Subscribe(x => { LandOfDeath.Dispose(); ServerManager.Instance.EnableMapEffect(98, false); });
                        fam.LandOfDeath = LandOfDeath;
                    }

                });
            }
        }

        public static TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan day = time;    // 24 hours in a day.
            TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));     // The current time in 24 hour format
            TimeSpan timeLeftUntilFirstRun = ((day - now));
            if (timeLeftUntilFirstRun.TotalHours < 0)
                timeLeftUntilFirstRun += new TimeSpan(24, 0, 0);
            return timeLeftUntilFirstRun;
        }

        public static void GenerateInstantBattle()
        {
            foreach (Schedule schedul in ServerManager.Schedules.Where(s => s.Event == "INSTANTBATTLE"))
            {
                Observable.Timer(TimeSpan.FromSeconds(EventHelper.GetMilisecondsBeforeTime(schedul.Time).TotalSeconds - (5 * 60)), TimeSpan.FromDays(1))
                .Subscribe(
                e =>
                {   //send 5min
                    Thread.Sleep(4 * 60 * 1000);
                    //send 1min
                    Thread.Sleep(30 * 1000);
                    //send 30sec
                    Thread.Sleep(20 * 1000);
                    //send 10sec
                    Thread.Sleep(5 * 1000);
                    //send 5sec
                    Thread.Sleep(5 * 1000);
                    //send icon
                    Thread.Sleep(30 * 1000);
                    //teleport
                    Thread.Sleep(60 * 1000);
                    //summon
                });
            }
        }
    }
}
