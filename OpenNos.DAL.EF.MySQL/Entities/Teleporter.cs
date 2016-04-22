namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Teleporter")]
    public partial class Teleporter
    {
        #region Properties

        public short Index { get; set; }
        public virtual Map Map { get; set; }
        public short MapId { get; set; }
        public virtual MapNpc MapNpc { get; set; }
        public int MapNpcId { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
        public short TeleporterId { get; set; }

        #endregion
    }
}