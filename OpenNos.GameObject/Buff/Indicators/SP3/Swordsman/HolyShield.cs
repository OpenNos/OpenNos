using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class HolyShield : IndicatorBase
    {
        #region Instantiation

        public HolyShield(int Level)
        {
            Name = "Holy Shield";
            Duration = 100;
            Id = 633;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.NeverCritical, 1, 0, false));
        }

        #endregion
    }
}