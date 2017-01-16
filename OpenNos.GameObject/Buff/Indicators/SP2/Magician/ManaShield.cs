using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class ManaShield : IndicatorBase
    {
        public ManaShield(int Level)
        {
            Name = "Mana Shield";
            Duration = 1800;
            Id = 88;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreasePercentage, 20, 0, false, true));
        }
    }
}
