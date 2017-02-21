using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Archer
{
    public class SinisterShadow : IndicatorBase
    {
        public SinisterShadow(int Level)
        {
            Name = "Sinister Shadow";
            Duration = 40;
            Id = 631;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.Increase, 10, 0, true));
        }
    }
}
