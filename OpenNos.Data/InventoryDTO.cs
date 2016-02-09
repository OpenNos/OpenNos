namespace OpenNos.Data
{
    public class InventoryDTO
    {
        #region Properties

        public long CharacterId { get; set; }

        public long InventoryId { get; set; }

        public long InventoryItemId { get; set; }

        public short Slot { get; set; }

        public short Type { get; set; }

        #endregion
    }
}