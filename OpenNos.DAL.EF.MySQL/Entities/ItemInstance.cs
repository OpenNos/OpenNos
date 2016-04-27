namespace OpenNos.DAL.EF.MySQL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ItemInstance
    {
        #region Instantiation

        public ItemInstance()
        {
        }

        #endregion

        #region Properties

        public int Amount { get; set; }

        public short Design { get; set; }
        public virtual Inventory Inventory { get; set; }

        public bool IsUsed { get; set; }

        public virtual Item Item { get; set; }

        public DateTime? ItemDeleteTime { get; set; }

        [Key, ForeignKey(nameof(Inventory))]
        public long ItemInstanceId { get; set; }

        public short ItemVNum { get; set; }
        public byte Rare { get; set; }
        public byte Upgrade { get; set; }

        #endregion
    }
}