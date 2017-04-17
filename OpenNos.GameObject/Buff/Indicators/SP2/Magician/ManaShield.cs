using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class ManaShield : IndicatorBase
    {
        #region Instantiation

        public ManaShield(int Level)
        {
            Name = "Mana Shield";
            Duration = 1800;
            Id = 88;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreasePercentage, 20, 0, false, true));
        }

        #endregion
    }
}