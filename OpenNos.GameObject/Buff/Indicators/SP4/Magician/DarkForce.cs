using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Magician
{
    public class DarkForce : IndicatorBase
    {
        public DarkForce(int Level)
        {
            Name = "Dark Force";
            Duration = 3000;
            Id = 157;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Element, BCard.SubType.IncreaseShadow, Level * 6, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Resistance, BCard.SubType.IncreaseShadow, 10, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.HitRate, BCard.SubType.Increase, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreaseCriticalChance, 20, 0, false, true));
        }
    }
}
