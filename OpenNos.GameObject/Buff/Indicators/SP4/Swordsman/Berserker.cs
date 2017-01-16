namespace OpenNos.GameObject.Buff.Indicators.SP4.Swordsman
{
    public class Berserker : IndicatorBase
    {
        public Berserker(int Level)
        {
            Name = "Berserker";
            Duration = 600;
            Id = 149;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseMelee, 400, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 2, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.Decrease, 200, 0, false));

            _level = Level;
            DelayedBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 4, 0, false));
            DelayedBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreasePercentage, 20, 0, true));
            Delay = 400;
        }
    }
}
