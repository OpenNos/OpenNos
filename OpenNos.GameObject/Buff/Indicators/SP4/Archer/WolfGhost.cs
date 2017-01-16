using System;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class WolfGhost : IndicatorBase
    {
        public WolfGhost(int Level)
        {
            Name = "Wolf Spirit";
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.Increase, Level * 4, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 2, 0, false));
            Duration = 3000;
            _level = Level;
            Id = 153;
        }
    }
}
