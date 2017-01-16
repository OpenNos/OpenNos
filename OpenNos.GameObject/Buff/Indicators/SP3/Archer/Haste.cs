using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Archer
{
    public class Haste : IndicatorBase
    {
        public Haste(int Level)
        {
            Name = "Haste";
            Duration = 1200;
            Id = 29;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 2, 0, false));
        }
    }
}
