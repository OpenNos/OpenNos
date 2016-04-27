namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class ShopItem
    {
        #region Properties

        public byte Color { get; set; }
        public virtual Item Item { get; set; }
        public short ItemVNum { get; set; }
        public byte Rare { get; set; }
        public virtual Shop Shop { get; set; }
        public int ShopId { get; set; }
        public int ShopItemId { get; set; }

        public byte Slot { get; set; }
        public byte Type { get; set; }
        public byte Upgrade { get; set; }

        #endregion
    }
}