using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ItemInstance : ItemInstanceDTO
    {
        public ItemInstance()
        {
            Mapper.CreateMap<ItemInstanceDTO, ItemInstance>();
            Mapper.CreateMap<ItemInstance, ItemInstanceDTO>();
        }
     

    }
}
