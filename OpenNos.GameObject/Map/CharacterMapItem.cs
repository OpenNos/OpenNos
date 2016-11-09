namespace OpenNos.GameObject
{
    public class CharacterMapItem : MapItem
    {
        #region Instantiation

        public CharacterMapItem(short x, short y, ItemInstance itemInstance) : base(x, y)
        {
            ItemInstance = itemInstance;
        }

        #endregion

        #region Properties

        public override byte Amount
        {
            get
            {
                return ItemInstance.Amount;
            }
            set
            {
                ItemInstance.Amount = Amount;
            }
        }

        public ItemInstance ItemInstance { get; set; }

        public override short ItemVNum
        {
            get
            {
                return ItemInstance.ItemVNum;
            }
            set
            {
                ItemInstance.ItemVNum = value;
            }
        }

        public override long TransportId
        {
            get
            {
                return ItemInstance.TransportId;
            }
            set
            {
                //cannot set TransportId
            }
        }

        #endregion

        #region Methods

        public override ItemInstance GetItemInstance()
        {
            return ItemInstance;
        }

        #endregion
    }
}