using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Archer
{
    public class CriticalHit : IndicatorBase
    {
        public CriticalHit(int Level)
        {
            Name = "Critical Hit";
            Duration = 50;
            Id = 92;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseCriticalChance, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseCriticalDamage, 50, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.Increase, 10, 0, true));
        }
    }
}
