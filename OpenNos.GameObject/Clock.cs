using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class Clock
    {
        public bool Enabled { get; private set; }
        public byte Type { get; set; }
        public int DeciSecondRemaining { get; set; }
        public int BasesSecondRemaining { get; set; }
        public List<EventContainer> StopEvents { get; set; }
        public List<EventContainer> TimeoutEvents { get; set; }

        public Clock(byte type)
        {
            StopEvents = new List<EventContainer>();
            TimeoutEvents = new List<EventContainer>();
            Type = type;
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(
           x =>
           {
               tick();
            });
        }

        public string GetClock()
        {
            return $"evnt {Type} {(Enabled ? 0 : -1)} {(int)(DeciSecondRemaining)} {(int)(BasesSecondRemaining)}";
        }
        private void tick()
        {
            if (Enabled)
            {
                if (DeciSecondRemaining > 0)
                {
                    DeciSecondRemaining-=10;
                }
                else
                {
                    TimeoutEvents.ForEach(ev =>
                    {
                        EventHelper.Instance.RunEvent(ev);
                    });
                    TimeoutEvents.RemoveAll(s => s != null);
                }
            }
        }
        public void StopClock()
        {
            Enabled = false;
            StopEvents.ForEach(e =>
            {
                EventHelper.Instance.RunEvent(e);
            });
            StopEvents.RemoveAll(s => s != null);
        }

        public void StartClock()
        {
            Enabled = true;
        }
    }
}