namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CellonOption")]
    public partial class CellonOption
    {
        #region Properties

        [Key]
        public int CellonOptionId { get; set; }

        [ForeignKey(nameof(WearableInstanceId))]
        public virtual WearableInstance WearableInstance { get; set; }

        public long WearableInstanceId { get; set; }

        public byte Level { get; set; }

        public byte Type { get; set; }

        public int Value { get; set; }

        #endregion
    }
}