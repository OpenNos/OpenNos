using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class Blessing : IndicatorBase
    {
        public Blessing(int Level)
        {
            Name = "Blessing";
            Duration = 3000;
            Id = 91;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.IncreaseLevel, 1, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Resistance, BCard.SubType.Increase, 5, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.IncreaseMelee, Level * 2, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.IncreaseDistance, Level * 2, 0, false));
        }
    }
}
