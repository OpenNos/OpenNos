using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class SecondBlessing : IndicatorBase
    {
        #region Instantiation

        public SecondBlessing(int Level)
        {
            Name = "The 2nd Triple Blessing";
            Duration = 150;
            Id = 141;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreasePercentage, 30, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.DecreaseCriticalDamage, 20, 0, false));
        }

        #endregion

        #region Methods

        public override void Disable(ClientSession session)
        {
            base.Disable(session);
            IndicatorBase buff = new ThirdBlessing(Level);
            session.Character.Buff.Add(buff);
        }

        #endregion
    }
}