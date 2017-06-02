namespace OpenNos.Master.Library.Data
{
    internal class AccountConnection
    {
        #region Instantiation

        public AccountConnection(long accountId, long session)
        {
        }

        #endregion

        #region Properties

        public long AccountId { get; private set; }

        public long CharacterId { get; set; }

        public WorldServer ConnectedWorld { get; set; }

        public long SessionId { get; private set; }

        #endregion
    }
}