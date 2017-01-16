using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Magician
{
    public class FireBlessing : IndicatorBase
    {
        public FireBlessing(int Level)
        {
            Name = "Fire Blessing";
            Duration = 3000;
            Id = 67;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Element, BCard.SubType.IncreaseFire, Level * 6, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Resistance, BCard.SubType.IncreaseFire, 25, 0, false));
        }
    }
}
