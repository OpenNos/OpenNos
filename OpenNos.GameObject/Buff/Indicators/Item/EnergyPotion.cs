using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.Item
{
    public class EnergyPotion : IndicatorBase
    {
        public EnergyPotion(int Level)
        {
            Name = "Energy Increase";
            Duration = 6000;
            Id = 118;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.HP, BCard.SubType.IncreasePercentage, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.MP, BCard.SubType.IncreasePercentage, 20, 0, false));
        }
    }
}
