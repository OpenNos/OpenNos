using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class FirstBlessing : IndicatorBase
    {
        public FirstBlessing(int Level)
        {
            Name = "The 1st Triple Blessing";
            Duration = 150;
            Id = 140;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreasePercentage, 10, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.DecreaseCriticalDamage, 10, 0, false));
        }

        public override void Disable(ClientSession session)
        {
            base.Disable(session);
            IndicatorBase buff = new SecondBlessing(_level);
            session.Character.Buff.Add(buff);
        }
    }
}
