using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Archer
{
    public class PactofDarkness : IndicatorBase
    {
        public PactofDarkness(int Level)
        {
            Name = "Pact of Darkness";
            Duration = 30;
            Id = 630;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalChance, 30, 0, false));
        }
    }
}
