using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Archer
{
    public class CriticalHit : IndicatorBase
    {
        #region Instantiation

        public CriticalHit(int Level)
        {
            Name = "Critical Hit";
            Duration = 50;
            Id = 92;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalChance, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalDamage, 50, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.Increase, 10, 0, true));
        }

        #endregion
    }
}