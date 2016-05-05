using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF.MySQL
{
    public class Inventory
    {
        #region Properties

        public virtual Character Character { get; set; }

        [Index("IX_SlotAndType", 1, IsUnique = true, Order = 0)]
        public long CharacterId { get; set; }

        public long InventoryId { get; set; }

        public virtual ItemInstance ItemInstance { get; set; }

        [Index("IX_SlotAndType", 2, IsUnique = true, Order = 1)]
        public short Slot { get; set; }

        [Index("IX_SlotAndType", 3, IsUnique = true, Order = 2)]
        public byte Type { get; set; }

        #endregion
    }
}