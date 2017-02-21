using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Archer
{
    public class BearSpirit : IndicatorBase
    {
        public BearSpirit(int Level)
        {
            Name = "Bear Spirit";
            Duration = 3000;
            Id = 155;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.HP, SubType.IncreasePercentage, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.MP, SubType.IncreasePercentage, 30, 0, false));
        }
    }
}
