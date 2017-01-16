using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool StaticBuff = false;
        public bool BadBuff = false;
        public int _level;
        public int _buffLevel = 1;
        public bool Disabled { get; private set; }
        public virtual void Enable(ClientSession session)
        {
            if (StaticBuff)
            {
                session.SendPacket($"vb {Id} 1 {Duration}");
                session.SendPacket(session.Character.GenerateSay($"You are under the effect {Name}.", 12));
            }
            else
            {
                session.SendPacket($"bf 1 {session.Character.CharacterId} 0.{Id}.{Duration} {_level}");
                session.SendPacket(session.Character.GenerateSay($"You are under the effect {Name}.", 20));
            }
            if (DirectBuffs.Any(s => s.Type == BCard.Type.Speed))
            {
                session.Character.LastSpeedChange = DateTime.Now;
                session.SendPacket(session.Character.GenerateCond());
            }
            if(Delay != -1)
            {
                System.Reactive.Linq.Observable.Timer(TimeSpan.FromMilliseconds(Duration * 100))
           .Subscribe(
           o =>
           {
               if (DelayedBuffs.Any(s => s.Type == BCard.Type.Speed))
               {
                   if (!Disabled && session != null && session.HasSelectedCharacter)
                   {
                       session.Character.LastSpeedChange = DateTime.Now;
                       session.SendPacket(session.Character.GenerateCond());
                   }
               }
           });
            }
            System.Reactive.Linq.Observable.Timer(TimeSpan.FromMilliseconds(Duration * 100))
                       .Subscribe(
                       o =>
                       {
                           Disable(session);
                       });
        }
        public virtual void Disable(ClientSession session)
        {
            if (!Disabled && session != null && session.HasSelectedCharacter)
            {
                if (StaticBuff)
                {
                    session.SendPacket($"vb {Id} 0 {Duration}");
                    session.SendPacket(session.Character.GenerateSay($"You are no longer under the effect {Name}.", 11));
                }
                else
                {
                    session.SendPacket($"bf 1 {session.Character.CharacterId} 0.{Id}.0 {_level}");
                    session.SendPacket(session.Character.GenerateSay($"You are no longer under the effect {Name}.", 20));
                }
                Disabled = true;
                if (DirectBuffs.Concat(DelayedBuffs).Any(s => s.Type == BCard.Type.Speed))
                {
                    session.Character.LastSpeedChange = DateTime.Now;
                    session.SendPacket(session.Character.GenerateCond());
                }
            }
        }
    }
}
