using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class ThirdBlessing : IndicatorBase
    {
        #region Instantiation

        public ThirdBlessing(int Level)
        {
            Name = "The 3rd Triple Blessing";
            Duration = 200;
            Id = 142;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreasePercentage, 50, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.DecreaseCriticalDamage, 30, 0, false));
        }

        #endregion

        #region Methods

        public override void Disable(ClientSession session)
        {
            base.Disable(session);
        }

        #endregion
    }
}