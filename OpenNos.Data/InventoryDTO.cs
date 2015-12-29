using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class InventoryDTO
    {
        public long InventoryId { get; set; }
        public long CharacterId { get; set; }
        public short Type { get; set; }
        public short Slot { get; set; }
        public long InventoryItemId { get; set; }

    }
}
