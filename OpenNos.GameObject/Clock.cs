using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Timers;

namespace OpenNos.GameObject
{
    public class Clock
    {
        Timer timer;
        public bool Enabled { get; private set; }
        public byte Type { get; set; }
        public int DeciSecondRemaining { get; set; }
        public int BasesSecondRemaining { get; set; }
        public List<EventContainer> StopEvents { get; set; }
        public List<EventContainer> TimeoutEvents { get; set; }

        public Clock(byte type)
        {
            Type = type;
        }

        public string GetClock()
        {
            return $"evnt {Type} {(Enabled ? 0 : -1)} {(int)(DeciSecondRemaining)} {(int)(BasesSecondRemaining)}";
        }
        public Clock()
        {
            timer = new Timer(100);
            timer.Elapsed += this.OnTimerElapsed;
            timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Enabled)
            {
                if (DeciSecondRemaining > 0)
                {
                    DeciSecondRemaining--;
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