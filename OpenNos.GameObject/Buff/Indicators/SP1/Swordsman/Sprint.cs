using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Swordsman
{
    public class Sprint : IndicatorBase
    {
        public Sprint(int Level)
        {
            Name = "Sprint";
            Duration = 100;
            Id = 93;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 5, 0, false));
        }
    }
}
