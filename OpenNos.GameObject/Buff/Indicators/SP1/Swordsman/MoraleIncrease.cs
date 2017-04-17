using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Swordsman
{
    public class MoraleIncrease : IndicatorBase
    {
        #region Instantiation

        public MoraleIncrease(int Level)
        {
            Name = "Morale Increase";
            DirectBuffs.Add(new BCardEntry(Type.Morale, SubType.Increase, Level, 0, false));
            Duration = 3600;
            base.Level = Level;
            Id = 72;
        }

        #endregion
    }
}