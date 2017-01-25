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
                            LOD.GenerateLod();
                            break;
                        case EventType.REPUTEVENT:
                            Reput.GenerateReput();
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
    }


}
