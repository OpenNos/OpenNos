namespace OpenNos.DAL.EF.MySQL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ItemInstance")]
    public partial class ItemInstance
    {
        #region Instantiation

        public ItemInstance()
        {
        }

        #endregion

        #region Properties

        public int Amount { get; set; }
        public virtual Inventory Inventory { get; set; }

        [Key, ForeignKey(nameof(Inventory))]
        public long ItemInstanceId { get; set; }

        public virtual Item Item { get; set; }
        public DateTime? ItemDeleteTime { get; set; }
        public short ItemVNum { get; set; }

        public bool IsUsed { get; set; }

        #endregion
    }
}