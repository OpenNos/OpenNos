using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class HolyWeapon : IndicatorBase
    {
        public HolyWeapon(int Level)
        {
            Name = "Holy Weapon";
            Duration = 3000;
            Id = 89;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Element, BCard.SubType.IncreaseLight, Level * 5, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Resistance, BCard.SubType.IncreaseLight, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.Increase, Level * 2, 0, false));
        }
    }
}
