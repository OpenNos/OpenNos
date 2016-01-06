using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class ShopItemDTO
    {
        public int ShopItemId { get; set; }
        public short Slot { get; set; }
        public short ItemVNum { get; set; }
        public short Upgrade { get; set; }
        public short Rare { get; set; }


    }
}
