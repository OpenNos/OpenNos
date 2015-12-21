using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class Inventory
    {
        public Inventory()
        {
            Mapper.CreateMap<InventoryDTO, Inventory>();
            Mapper.CreateMap<Inventory, InventoryDTO>();
        }
        public long InventoryId { get; set; }
        public long CharacterId { get; set; }
        public short Type { get; set; }
        public short Slot { get; set; }
        public ItemInstance Item { get; set; }

    }
}
