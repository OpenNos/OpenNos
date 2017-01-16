namespace OpenNos.GameObject.Buff.Indicators.SP2.Swordsman
{
    public class SharpEdge : IndicatorBase
    {
        public SharpEdge(int Level)
        {
            Name = "Sharp Edge";
            Duration = 1200;
            Id = 80;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseLevel, 2, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseCriticalChance, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Dodge, BCard.SubType.IncreaseDistance, Level * 3, 0, false));
        }
    }
}
