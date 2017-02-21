using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Archer
{
    public class MiraclousHealing : IndicatorBase
    {
        public MiraclousHealing(int Level)
        {
            Name = "Miraclous Healing";
            Duration = 1800;
            Id = 328;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreasePercentage, 15, 0, false));
        }
    }
}
