using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Magician
{
    public class GhostGuard : IndicatorBase
    {
        public GhostGuard(int Level)
        {
            Name = "Ghost Guard";
            Duration = 3000;
            Id = 156;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 4, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseCriticalDamage, 50, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseCriticalChance, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreasePercentage, 30, 0, false, true));
        }
    }
}
