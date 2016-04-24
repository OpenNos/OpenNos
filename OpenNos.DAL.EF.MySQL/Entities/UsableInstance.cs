using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF.MySQL
{
    [Table(nameof(UsableInstance))]
    public class UsableInstance : ItemInstance
    {
        #region Properties

        public short HP { get; set; }
        public short MP { get; set; }

        #endregion
    }
}