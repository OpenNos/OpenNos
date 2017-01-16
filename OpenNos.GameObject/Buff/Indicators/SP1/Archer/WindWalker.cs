using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Archer
{
    public class WindWalker : IndicatorBase
    {
        public WindWalker(int Level)
        {
            Name = "Wind Walker";
            Duration = 4200;
            Id = 75;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 2, 0, false));
        }
    }
}
