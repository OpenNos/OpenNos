using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Magician
{
    public class GhostGuard : IndicatorBase
    {
        #region Instantiation

        public GhostGuard(int Level)
        {
            Name = "Ghost Guard";
            Duration = 3000;
            Id = 156;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 4, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalDamage, 50, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalChance, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreasePercentage, 30, 0, false, true));
        }

        #endregion
    }
}