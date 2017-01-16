using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Archer
{
    public class SinisterShadow : IndicatorBase
    {
        public SinisterShadow(int Level)
        {
            Name = "Sinister Shadow";
            Duration = 40;
            Id = 631;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.Increase, 10, 0, true));
        }
    }
}
