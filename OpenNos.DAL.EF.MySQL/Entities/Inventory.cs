namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Inventory
    {
        #region Properties

        public virtual Character Character { get; set; }

        public long CharacterId { get; set; }

        [Key]
        public long InventoryId { get; set; }

        public virtual ItemInstance ItemInstance { get; set; }

        public short Slot { get; set; }
        public byte Type { get; set; }

        #endregion
    }
}