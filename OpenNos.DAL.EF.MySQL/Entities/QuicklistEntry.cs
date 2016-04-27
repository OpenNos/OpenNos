namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class QuicklistEntry
    {
        #region Properties

        public virtual Character Character { get; set; }

        public long CharacterId { get; set; }

        [Key]
        public long EntryId { get; set; }

        public short Pos { get; set; }
        public short Q1 { get; set; }

        public short Q2 { get; set; }

        public short Slot { get; set; }
        public short Type { get; set; }

        #endregion
    }
}