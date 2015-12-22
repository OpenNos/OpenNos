using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class Item : ItemDTO
    {
        public Item()
        {
            Mapper.CreateMap<ItemDTO, Item>();
            Mapper.CreateMap<Item, ItemDTO>();
        }
      

    }
}
