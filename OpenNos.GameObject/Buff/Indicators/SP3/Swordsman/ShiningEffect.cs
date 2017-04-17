namespace OpenNos.GameObject.Buff.Indicators.SP3.Swordsman
{
    public class ShiningEffect : IndicatorBase
    {
        #region Instantiation

        public ShiningEffect(int Level)
        {
            Name = "Shining Effect";
            Duration = 1;
            Id = 0;

            base.Level = Level;
        }

        #endregion

        #region Methods

        public override void Enable(ClientSession session)
        {
            base.Enable(session);
            int hpbonus = Level * 25;
            if (session.Character.Hp + hpbonus <= session.Character.HPLoad())
            {
                session.Character.Hp += hpbonus;
            }
            else
            {
                hpbonus = (int)session.Character.HPLoad() - session.Character.Hp;
                session.Character.Hp = (int)session.Character.HPLoad();
            }
            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateRc(hpbonus));
            session.SendPacket(session.Character.GenerateStat());
            Disable(session);
        }

        #endregion
    }
}