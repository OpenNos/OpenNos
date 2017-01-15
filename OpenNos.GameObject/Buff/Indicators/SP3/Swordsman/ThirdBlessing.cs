using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class ThirdBlessing : IndicatorBase
    {
        public ThirdBlessing(int Level)
        {
            Name = "The 3rd Triple Blessing";
            Duration = 200;
            Id = 142;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreasePercentage, 50, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.DecreaseCriticalDamage, 30, 0, false));
        }

        public override void Disable(ClientSession session)
        {
            base.Disable(session);
        }
    }
}
