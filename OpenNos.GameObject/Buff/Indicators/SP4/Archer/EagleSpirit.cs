using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class EagleSpirit : IndicatorBase
    {
        public EagleSpirit(int Level)
        {
            Name = "Eagle Spirit";
            Duration = 1800;
            Id = 151;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.HitRate, BCard.SubType.Increase, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseCriticalChance, 10, 0, false));
        }
    }
}
