namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CellonOption")]
    public partial class CellonOption
    {
        #region Properties

        public int CellonOptionId { get; set; }

        public virtual InventoryItem InventoryItem { get; set; }
        public long InventoryItemId { get; set; }

        public byte Level { get; set; }

        public byte Type { get; set; }

        public int Value { get; set; }

        #endregion
    }
}