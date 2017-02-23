using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Swordsman
{
    public class SharpEdge : IndicatorBase
    {
        #region Instantiation

        public SharpEdge(int Level)
        {
            Name = "Sharp Edge";
            Duration = 1200;
            Id = 80;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseLevel, 2, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseCriticalChance, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Dodge, SubType.IncreaseDistance, Level * 3, 0, false));
        }

        #endregion
    }
}