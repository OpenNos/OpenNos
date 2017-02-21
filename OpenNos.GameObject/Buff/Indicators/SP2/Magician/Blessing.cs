using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class Blessing : IndicatorBase
    {
        public Blessing(int Level)
        {
            Name = "Blessing";
            Duration = 3000;
            Id = 91;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.IncreaseLevel, 1, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Resistance, SubType.Increase, 5, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.IncreaseMelee, Level * 2, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.IncreaseDistance, Level * 2, 0, false));
        }
    }
}
