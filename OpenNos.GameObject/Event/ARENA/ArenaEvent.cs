using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace OpenNos.GameObject.Event.ARENA
{
    class ArenaEvent
    {
        internal static void GenerateTalentArena()
        {
            Observable.Timer(TimeSpan.FromMinutes(0)).Subscribe(X =>
            {
                int seconds = 0;
                while (seconds < 60 * 60 * 7)
                {
                    seconds++;
                    Thread.Sleep(1000);
                }
                ServerManager.Instance.StartedEvents.Remove(EventType.TALENTARENA);
            });
        }
    }
}