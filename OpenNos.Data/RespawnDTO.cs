namespace OpenNos.Data
{
    public class RespawnDTO
    {
        #region Properties

        public long CharacterId { get; set; }

        public long RespawnId { get; set; }

        public short MapId { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        public short TeleportType { get; set; }

        #endregion
    }
}
