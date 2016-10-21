using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class CharacterMapItem : MapItem
    {
        public CharacterMapItem(short x, short y, ItemInstance itemInstance) : base(x, y)
        {
            ItemInstance = itemInstance;
        }

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

        public ItemInstance ItemInstance { get;set; }

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

        public override ItemInstance GetItemInstance()
        {
            return ItemInstance;
        }
    }
}
