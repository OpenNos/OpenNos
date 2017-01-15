namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class HolyShield : IndicatorBase
    {
        public HolyShield(int Level)
        {
            Name = "Holy Shield";
            Duration = 100;
            Id = 633;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.NeverCritical, 1, 0, false));
        }
    }
}
