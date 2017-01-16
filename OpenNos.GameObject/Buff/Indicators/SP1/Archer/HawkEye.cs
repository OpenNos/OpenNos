using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Archer
{
    public class HawkEye : IndicatorBase
    {
        public HawkEye(int Level)
        {
            Name = "Hawk Eye";
            Duration = 3000;
            Id = 74;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Effect, BCard.SubType.EagleEyes, 1, 0, false));
        }
    }
}
