using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Swordsman
{
    public class BreathofRecovery : IndicatorBase
    {
        public BreathofRecovery(int Level)
        {
            Name = "Breath of Recovery";
            Duration = 1;
            Id = 0;

            _level = Level;
        }
        public override void Enable(ClientSession session)
        {
            base.Enable(session);
            int hpbonus = _level * 50;
            if (session.Character.Hp + hpbonus <= session.Character.HPLoad())
            {
                session.Character.Hp += hpbonus;
            }
            else
            {
                hpbonus = (int)session.Character.HPLoad() - session.Character.Hp;
                session.Character.Hp = (int)session.Character.HPLoad();
            }
            int mpbonus = _level * 20;
            if (session.Character.Mp + mpbonus <= session.Character.MPLoad())
            {
                session.Character.Mp += mpbonus;
            }
            else
            {
                mpbonus = (int)session.Character.MPLoad() - session.Character.Mp;
                session.Character.Mp = (int)session.Character.MPLoad();
            }
            session.Character.Buff.DisableEffects(false, true, 4);
            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRc(hpbonus));
            session.SendPacket(session.Character.GenerateStat());
            Disable(session);
        }
    }
}
