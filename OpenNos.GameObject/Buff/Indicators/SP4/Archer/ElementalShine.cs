using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class ElementalShine : IndicatorBase
    {
        public ElementalShine(int Level)
        {
            Name = "Elemental Shine";
            Duration = 3000;
            Id = 152;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Resistance, BCard.SubType.IncreaseLight, 30, 0, false));
        }
    }
}
