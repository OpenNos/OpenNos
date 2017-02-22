using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Magician
{
    public class BlessingofWater : IndicatorBase
    {
        #region Instantiation

        public BlessingofWater(int Level)
        {
            Name = "Blessing of Water";
            Duration = 3000;
            Id = 134;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Element, SubType.IncreaseWater, Level * 6, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Resistance, SubType.IncreaseWater, 25, 0, false));
        }

        #endregion
    }
}