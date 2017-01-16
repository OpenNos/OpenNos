using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Magician
{
    public class ManaTransfusion : IndicatorBase
    {
        public ManaTransfusion(int Level)
        {
            Name = "Mana Transfusion";
            Duration = 1800;
            Id = 370;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreasePercentage, 20, 0, false, true));
        }
    }
}
