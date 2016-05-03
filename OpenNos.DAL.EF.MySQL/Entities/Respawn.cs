namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Respawn
    {
        #region Properties

        public virtual Character Character { get; set; }
        public long CharacterId { get; set; }
        public short MapId { get; set; }
        public long RespawnId { get; set; }

        public byte RespawnType { get; set; }
        public short X { get; set; }

        public short Y { get; set; }

        #endregion
    }
}