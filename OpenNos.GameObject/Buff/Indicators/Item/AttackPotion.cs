using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.Item
{
    public class AttackPotion : IndicatorBase
    {
        public AttackPotion(int Level)
        {
            Name = "Attack Enhancement";
            Duration = 6000;
            Id = 116;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreasePercentage, 20, 0, false));
        }
    }
}
