using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class EagleSpirit : IndicatorBase
    {
        #region Instantiation

        public EagleSpirit(int Level)
        {
            Name = "Eagle Spirit";
            Duration = 1800;
            Id = 151;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.HitRate, SubType.Increase, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalChance, 10, 0, false));
        }

        #endregion
    }
}