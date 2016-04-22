namespace OpenNos.DAL.EF.MySQL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("InventoryItem")]
    public partial class InventoryItem
    {
        #region Instantiation

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InventoryItem()
        {
            CellonOption = new HashSet<CellonOption>();
        }

        #endregion

        #region Properties

        public byte Ammo { get; set; }
        public int Amount { get; set; }
        public byte Cellon { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CellonOption> CellonOption { get; set; }

        public int CellonOptionId { get; set; }
        public short CloseDefence { get; set; }
        public short Concentrate { get; set; }
        public short CriticalDodge { get; set; }
        public byte CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public short DamageMaximum { get; set; }
        public short DamageMinimum { get; set; }
        public byte DarkElement { get; set; }
        public sbyte DarkResistance { get; set; }
        public short DefenceDodge { get; set; }
        public short Design { get; set; }
        public short DistanceDefence { get; set; }
        public short DistanceDefenceDodge { get; set; }
        public short ElementRate { get; set; }
        public byte FireElement { get; set; }
        public sbyte FireResistance { get; set; }
        public short HitRate { get; set; }
        public short HP { get; set; }

        public virtual Inventory Inventory { get; set; }

        [Key, ForeignKey(nameof(Inventory))]
        public long InventoryItemId { get; set; }

        public bool IsEmpty { get; set; }
        public bool IsFixed { get; set; }
        public bool IsUsed { get; set; }
        public virtual Item Item { get; set; }
        public DateTime? ItemDeleteTime { get; set; }
        public short ItemVNum { get; set; }
        public byte LightElement { get; set; }
        public sbyte LightResistance { get; set; }
        public short MagicDefence { get; set; }
        public short MP { get; set; }
        public byte Rare { get; set; }

        public short SlDamage { get; set; }
        public short SlDefence { get; set; }
        public short SlElement { get; set; }
        public short SlHP { get; set; }
        public byte SpDamage { get; set; }
        public byte SpDark { get; set; }
        public byte SpDefence { get; set; }
        public byte SpElement { get; set; }
        public byte SpFire { get; set; }
        public byte SpHP { get; set; }
        public byte SpLevel { get; set; }
        public byte SpLight { get; set; }
        public byte SpStoneUpgrade { get; set; }
        public byte SpWater { get; set; }
        public long SpXp { get; set; }
        public byte Upgrade { get; set; }
        public byte WaterElement { get; set; }
        public sbyte WaterResistance { get; set; }

        #endregion
    }
}