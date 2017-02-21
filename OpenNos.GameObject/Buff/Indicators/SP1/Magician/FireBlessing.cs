using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Magician
{
    public class FireBlessing : IndicatorBase
    {
        public FireBlessing(int Level)
        {
            Name = "Fire Blessing";
            Duration = 3000;
            Id = 67;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Element, SubType.IncreaseFire, Level * 6, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Resistance, SubType.IncreaseFire, 25, 0, false));
        }
    }
}
