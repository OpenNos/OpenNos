using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Magician
{
    public class FrozenShield : IndicatorBase
    {
        #region Instantiation

        public FrozenShield(int Level)
        {
            Name = "Frozen Shield";
            Duration = 30;
            Id = 144;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreasePercentage, 100, 0, false, true));
        }

        #endregion
    }
}