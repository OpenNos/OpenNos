using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event
{
    public class IceBreaker
    {
        public const int MAX_ALLOWED_PLAYERS = 50;

        private static readonly Tuple<int, int>[] _levelBrackets =
        {
            new Tuple<int, int>(1, 25),
            new Tuple<int, int>(20, 40),
            new Tuple<int, int>(35, 55),
            new Tuple<int, int>(50, 70),
            new Tuple<int, int>(65, 85),
            new Tuple<int, int>(80, 99),
        };

        private static int _currentBracket = 0;

        public static List<ClientSession> AlreadyFrozenPlayers { get; set; }

        public static List<ClientSession> FrozenPlayers { get; set; }

        public static MapInstance Map { get; private set; }

        public static void GenerateIceBreaker()
        {
            AlreadyFrozenPlayers = new List<ClientSession>();
            Map = ServerManager.Instance.GenerateMapInstance(2005, MapInstanceType.IceBreakerInstance, new InstanceBag());
            /*ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ICEBREAKER_MINUTES"), 5, _levelBrackets[_currentBracket].Item1, _levelBrackets[_currentBracket].Item2), 1));
            Thread.Sleep(5 * 60 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ICEBREAKER_MINUTES"), 1, _levelBrackets[_currentBracket].Item1, _levelBrackets[_currentBracket].Item2), 1));
            Thread.Sleep(1 * 60 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ICEBREAKER_SECONDS"), 30, _levelBrackets[_currentBracket].Item1, _levelBrackets[_currentBracket].Item2), 1));
            Thread.Sleep(30 * 1000);*/
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ICEBREAKER_SECONDS"), 10, _levelBrackets[_currentBracket].Item1, _levelBrackets[_currentBracket].Item2), 1));
            Thread.Sleep(10 * 1000);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_STARTED"), 1));
            ServerManager.Instance.IceBreakerInWaiting = true;
            ServerManager.Instance.Sessions.Where(x => x.Character.Level >= _levelBrackets[_currentBracket].Item1 && x.Character.Level <= _levelBrackets[_currentBracket].Item2 && x.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance).ToList().ForEach(x => x.SendPacket($"qnaml 2 #guri^501 {string.Format(Language.Instance.GetMessageFromKey("ICEBREAKER_ASK"), 500)}"));
            _currentBracket++;
            if (_currentBracket > 5)
            {
                _currentBracket = 0;
            }
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(c =>
            {
                ServerManager.Instance.StartedEvents.Remove(EventType.ICEBREAKER);
                ServerManager.Instance.IceBreakerInWaiting = false;
                if (Map.Sessions.Count() <= 1)
                {
                    int goldReward = 15000;  //I STILL DON'T KNOW HOW THE GOLD REWARD IS CALCULATED
                    Map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_WIN"), 0));
                    Map.Sessions.ToList().ForEach(x =>
                    {
                        x.Character.GetReput(x.Character.Level * 10);
                        x.Character.Gold += goldReward;
                        x.Character.Gold = x.Character.Gold > ServerManager.Instance.MaxGold ? ServerManager.Instance.MaxGold : x.Character.Gold;
                        x.SendPacket(x.Character.GenerateGold());
                        x.SendPacket(x.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_MONEY"), goldReward), 10));
                        x.SendPacket(x.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_REPUT"), x.Character.Level * 10), 10));
                    });
                    Thread.Sleep(5000);
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10), new EventContainer(Map, EventActionType.DISPOSEMAP, null));
                }
                else
                {
                    Map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_FIGHT_WARN"), 0));
                    Thread.Sleep(6000);
                    Map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_FIGHT_WARN"), 0));
                    Thread.Sleep(7000);
                    Map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_FIGHT_WARN"), 0));
                    Thread.Sleep(1000);
                    Map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_FIGHT_START"), 0));
                    Map.IsPVP = true;
                    while (Map.Sessions.Count() > 1 || AlreadyFrozenPlayers.Count() != Map.Sessions.Count())
                    {
                        Thread.Sleep(1000);
                    }
                    Map.IsPVP = false;
                    int goldReward = 15000;  //I STILL DON'T KNOW HOW THE GOLD REWARD IS CALCULATED
                    Map.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ICEBREAKER_WIN"), 0));
                    Map.Sessions.ToList().ForEach(x =>
                    {
                        x.Character.GetReput(x.Character.Level * 10);
                        x.Character.Gold += goldReward;
                        x.Character.Gold = x.Character.Gold > ServerManager.Instance.MaxGold ? ServerManager.Instance.MaxGold : x.Character.Gold;
                        x.SendPacket(x.Character.GenerateGold());
                        x.SendPacket(x.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_MONEY"), goldReward), 10));
                        x.SendPacket(x.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_REPUT"), x.Character.Level * 10), 10));
                    });
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10), new EventContainer(Map, EventActionType.DISPOSEMAP, null));
                }
            });
        }
    }
}
