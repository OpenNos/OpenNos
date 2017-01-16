using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Magician
{
    public class BlessingofWater : IndicatorBase
    {
        public BlessingofWater(int Level)
        {
            Name = "Blessing of Water";
            Duration = 3000;
            Id = 134;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Element, BCard.SubType.IncreaseWater, Level * 6, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Resistance, BCard.SubType.IncreaseWater, 25, 0, false));
        }
    }
}
