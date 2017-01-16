using System;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Swordsman
{
    public class MoraleIncrease : IndicatorBase
    {
        public MoraleIncrease(int Level)
        {
            Name = "Morale Increase";
            DirectBuffs.Add(new BCardEntry(BCard.Type.Morale, BCard.SubType.Increase, Level, 0, false));
            Duration = 3600;
            _level = Level;
            Id = 72;
        }
    }
}
