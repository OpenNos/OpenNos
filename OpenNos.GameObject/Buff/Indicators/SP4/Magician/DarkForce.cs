using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Magician
{
    public class DarkForce : IndicatorBase
    {
        public DarkForce(int Level)
        {
            Name = "Dark Force";
            Duration = 3000;
            Id = 157;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Element, SubType.IncreaseShadow, Level * 6, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Resistance, SubType.IncreaseShadow, 10, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.HitRate, SubType.Increase, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreaseCriticalChance, 20, 0, false, true));
        }
    }
}
