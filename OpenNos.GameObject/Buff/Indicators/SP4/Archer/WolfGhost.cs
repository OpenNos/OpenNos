using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class WolfGhost : IndicatorBase
    {
        #region Instantiation

        public WolfGhost(int Level)
        {
            Name = "Wolf Spirit";
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.Increase, Level * 4, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 2, 0, false));
            Duration = 3000;
            _level = Level;
            Id = 153;
        }

        #endregion
    }
}