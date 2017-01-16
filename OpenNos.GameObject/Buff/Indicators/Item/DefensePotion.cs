using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.Item
{
    public class DefensePotion : IndicatorBase
    {
        public DefensePotion(int Level)
        {
            Name = "Armor Enhancement";
            Duration = 6000;
            Id = 117;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.IncreasePercentage, 20, 0, false));
        }
    }
}
