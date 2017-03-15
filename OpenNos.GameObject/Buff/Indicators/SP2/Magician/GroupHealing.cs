namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class GroupHealing : IndicatorBase
    {
        #region Instantiation

        public GroupHealing(int Level)
        {
            Name = "Group Healing";
            Duration = 1;
            Id = 0;

            _level = Level;
        }

        #endregion

        #region Methods

        public override void Enable(ClientSession session)
        {
            base.Enable(session);
            int hpbonus = _level * 55;
            if (session.Character.Hp + hpbonus <= session.Character.HPLoad())
            {
                session.Character.Hp += hpbonus;
            }
            else
            {
                hpbonus = (int)session.Character.HPLoad() - session.Character.Hp;
                session.Character.Hp = (int)session.Character.HPLoad();
            }
            session.CurrentMapInstanceNode?.Data.Broadcast(session, session.Character.GenerateRc(hpbonus));
            session.SendPacket(session.Character.GenerateStat());
            Disable(session);
        }

        #endregion
    }
}