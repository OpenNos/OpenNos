namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class PrayerofDefence : IndicatorBase
    {
        public PrayerofDefence(int Level)
        {
            Name = "Prayer of Defence";
            Duration = 1800;
            Id = 138;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.HP, BCard.SubType.IncreasePercentage, 15, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.MP, BCard.SubType.IncreasePercentage, 15, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Defense, BCard.SubType.IncreaseLevel, 1, 0, false));
        }
    }
}
