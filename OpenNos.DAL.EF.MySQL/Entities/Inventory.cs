namespace OpenNos.DAL.EF.MySQL
{
    public class Inventory
    {
        #region Properties

        public virtual Character Character { get; set; }

        public long CharacterId { get; set; }

        public long InventoryId { get; set; }

        public virtual ItemInstance ItemInstance { get; set; }

        public short Slot { get; set; }

        public byte Type { get; set; }

        #endregion
    }
}