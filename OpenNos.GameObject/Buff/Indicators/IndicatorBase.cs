using System;
using System.Collections.Generic;

namespace OpenNos.GameObject.Buff.Indicators
{
    public class IndicatorBase
    {
        public string Name;
        public List<BCardEntry> DirectBuffs = new List<BCardEntry>();
        public List<BCardEntry> DelayedBuffs = new List<BCardEntry>();
        public int Delay = -1;
        public DateTime Start = DateTime.Now;
        public int Duration;
        public int Id;
        public int Interval = -1;
        public bool Disabled { get; private set; }
        public virtual void Enable(ClientSession session)
        {
            session.SendPacket($"bf 1 {session.Character.CharacterId} 0.{Id}.{Duration} {session.Character.Level}");
            session.SendPacket(session.Character.GenerateSay($"You are under the effect {Name}.", 20));
            System.Reactive.Linq.Observable.Timer(TimeSpan.FromMilliseconds(Duration * 100))
                       .Subscribe(
                       o =>
                       {
                           Disable(session);
                       });
        }
        public virtual void Disable(ClientSession session)
        {
            if (!Disabled)
            {
                session.SendPacket($"bf 1 {session.Character.CharacterId} 0.{Id}.0 {session.Character.Level}");
                session.SendPacket(session.Character.GenerateSay($"You are no longer under the effect {Name}.", 20));
                Disabled = true;
            }
        }
    }
}
