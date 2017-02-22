using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Archer
{
    public class BoosterOn : IndicatorBase
    {
        #region Instantiation

        public BoosterOn(int Level)
        {
            Name = "Booster On";
            Duration = 50;
            Id = 136;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 10, 0, false));
        }

        #endregion

        #region Methods

        public override void Disable(ClientSession session)
        {
            base.Disable(session);
            IndicatorBase buff = new Haste(_level);
            session.Character.Buff.Add(buff);
        }

        #endregion
    }
}