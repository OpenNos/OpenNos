using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class BearSpirit : IndicatorBase
    {
        public BearSpirit(int Level)
        {
            Name = "Bear Spirit";
            Duration = 3000;
            Id = 155;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.HP, BCard.SubType.IncreasePercentage, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.MP, BCard.SubType.IncreasePercentage, 30, 0, false));
        }
    }
}
