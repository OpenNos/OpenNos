namespace OpenNos.DAL.EF.MySQL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("GeneralLog")]
    public partial class GeneralLog
    {
        #region Properties

        public virtual Account Account { get; set; }

        public long AccountId { get; set; }

        public virtual Character Character { get; set; }

        public long? CharacterId { get; set; }

        [MaxLength(255)]
        public string IpAddress { get; set; }

        [MaxLength(255)]
        public string LogData { get; set; }

        [Key]
        public long LogId { get; set; }

        public string LogType { get; set; }
        public DateTime Timestamp { get; set; }

        #endregion
    }
}