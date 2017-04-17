using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Swordsman
{
    public class Sprint : IndicatorBase
    {
        #region Instantiation

        public Sprint(int Level)
        {
            Name = "Sprint";
            Duration = 100;
            Id = 93;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 5, 0, false));
        }

        #endregion
    }
}