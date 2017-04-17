using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class PrayerofDefence : IndicatorBase
    {
        #region Instantiation

        public PrayerofDefence(int Level)
        {
            Name = "Prayer of Defence";
            Duration = 1800;
            Id = 138;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.HP, SubType.IncreasePercentage, 15, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.MP, SubType.IncreasePercentage, 15, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.IncreaseLevel, 1, 0, false));
        }

        #endregion
    }
}