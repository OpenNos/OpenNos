namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class Healing : IndicatorBase
    {
        #region Instantiation

        public Healing(int Level)
        {
            Name = "Healing";
            Duration = 1;
            Id = 0;

            base.Level = Level;
        }

        #endregion

        #region Methods

        public override void Enable(ClientSession session)
        {
            base.Enable(session);
            int hpbonus = Level * 20;
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