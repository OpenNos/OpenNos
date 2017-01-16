using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Magician
{
    public class FrozenShield : IndicatorBase
    {
        public FrozenShield(int Level)
        {
            Name = "Frozen Shield";
            Duration = 30;
            Id = 144;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreasePercentage, 100, 0, false, true));
        }
    }
}
