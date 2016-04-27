using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF.MySQL
{
    public class WearableInstance : ItemInstance
    {
        #region Properties

        public byte? Ammo { get; set; }
        public byte? Cellon { get; set; }
        public virtual ICollection<CellonOption> CellonOption { get; set; }
        public int? CellonOptionId { get; set; }
        public short? CloseDefence { get; set; }
        public short? Concentrate { get; set; }
        public short? CriticalDodge { get; set; }
        public byte? CriticalLuckRate { get; set; }
        public short? CriticalRate { get; set; }
        public short? DamageMaximum { get; set; }
        public short? DamageMinimum { get; set; }
        public byte? DarkElement { get; set; }
        public sbyte? DarkResistance { get; set; }
        public short? DefenceDodge { get; set; }
        public short? DistanceDefence { get; set; }
        public short? DistanceDefenceDodge { get; set; }
        public short? ElementRate { get; set; }
        public byte? FireElement { get; set; }
        public sbyte? FireResistance { get; set; }
        public short? HitRate { get; set; }
        public bool? IsEmpty { get; set; }
        public bool? IsFixed { get; set; }
        public byte? LightElement { get; set; }
        public sbyte? LightResistance { get; set; }
        public short? MagicDefence { get; set; }
        public byte? WaterElement { get; set; }
        public sbyte? WaterResistance { get; set; }
        public short? HP { get; set; }
        public short? MP { get; set; }

        #endregion
    }
}