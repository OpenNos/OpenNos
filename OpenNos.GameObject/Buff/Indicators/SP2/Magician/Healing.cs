using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class Healing : IndicatorBase
    {
        public Healing(int Level)
        {
            Name = "Healing";
            Duration = 1;
            Id = 0;

            _level = Level;
        }
        public override void Enable(ClientSession session)
        {
            base.Enable(session);
            int hpbonus = _level * 20;
            if (session.Character.Hp + hpbonus <= session.Character.HPLoad())
            {
                session.Character.Hp += hpbonus;
            }
            else
            {
                hpbonus = (int)session.Character.HPLoad() - session.Character.Hp;
                session.Character.Hp = (int)session.Character.HPLoad();
            }
            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRc(hpbonus));
            session.SendPacket(session.Character.GenerateStat());
            Disable(session);
        }
    }
}
