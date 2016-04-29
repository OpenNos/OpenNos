namespace OpenNos.DAL.EF.MySQL
{
    using System;

    public class ItemInstance
    {
        #region Properties

        public int Amount { get; set; }

        public short Design { get; set; }

        public virtual Inventory Inventory { get; set; }

        public bool IsUsed { get; set; }

        public virtual Item Item { get; set; }

        public DateTime? ItemDeleteTime { get; set; }

        public long ItemInstanceId { get; set; }

        public short ItemVNum { get; set; }

        public byte Rare { get; set; }

        public byte Upgrade { get; set; }

        #endregion
    }
}