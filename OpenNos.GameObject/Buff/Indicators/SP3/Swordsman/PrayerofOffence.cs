namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class PrayerofOffence : IndicatorBase
    {
        public PrayerofOffence(int Level)
        {
            Name = "Prayer of Defence";
            Duration = 1800;
            Id = 139;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Damage, BCard.SubType.IncreaseLevel, 2, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.Morale, BCard.SubType.Increase, 5, 0, false));
        }
    }
}
