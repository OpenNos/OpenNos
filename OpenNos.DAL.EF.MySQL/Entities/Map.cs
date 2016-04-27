namespace OpenNos.DAL.EF.MySQL
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Map
    {
        #region Instantiation

        public Map()
        {
            Character = new HashSet<Character>();
            MapMonster = new HashSet<MapMonster>();
            MapNpc = new HashSet<MapNpc>();
            Portal = new HashSet<Portal>();
            Portal1 = new HashSet<Portal>();
            Teleporter = new HashSet<Teleporter>();
        }

        #endregion

        #region Properties

        public virtual ICollection<Character> Character { get; set; }

        public byte[] Data { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short MapId { get; set; }

        public virtual ICollection<MapMonster> MapMonster { get; set; }

        public virtual ICollection<MapNpc> MapNpc { get; set; }

        public int Music { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public virtual ICollection<Portal> Portal { get; set; }

        public virtual ICollection<Portal> Portal1 { get; set; }

        public virtual ICollection<Teleporter> Teleporter { get; set; }

        #endregion
    }
}