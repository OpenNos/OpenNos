using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Swordsman
{
    public class IronSkin : IndicatorBase
    {
        public IronSkin(int Level)
        {
            Name = "Iron Skin";
            Duration = 300;
            Id = 71;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreaseMeleePercentage, 20, 0, false, true));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.DecreaseDistancePercentage, 65, 0, false, true));
            DelayedBuffs.Add(new BCardEntry(BCard.Type.Cooldown, BCard.SubType.DecreasePercentage, 15, 0, false));
            Delay = 2;
        }
    }
}
