using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Archer
{
    public class Haste : IndicatorBase
    {
        #region Instantiation

        public Haste(int Level)
        {
            Name = "Haste";
            Duration = 1200;
            Id = 29;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 2, 0, false));
        }

        #endregion
    }
}