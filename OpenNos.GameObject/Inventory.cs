using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class Inventory
    {
        public long InventoryId { get; set; }
        public long CharacterId { get; set; }
        public short Type { get; set; }
        public short Slot { get; set; }
        public Item Item { get; set; }

    }
}
