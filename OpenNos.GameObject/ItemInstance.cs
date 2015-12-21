using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ItemInstance
    {
        public ItemInstance()
        {
            Mapper.CreateMap<ItemInstanceDTO, ItemInstance>();
            Mapper.CreateMap<ItemInstance, ItemInstanceDTO>();
        }
        public short ItemId { get; set; }
        public short DamageMinimum { get; set; }
        public short DamageMaximum { get; set; }
        public short Concentrate { get; set; }
        public short HitRate { get; set; }
        public short CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public short RangeDefence { get; set; }
        public short DistanceDefence { get; set; }
        public short MagicDefence { get; set; }
        public string Dodge { get; set; }
        public short ElementRate { get; set; }
        public short Upgrade { get; set; }
        public short Rare { get; set; }
        public string Color { get; set; }
        public string Amount { get; set; }
        public short Level { get; set; }
        public short SlElement { get; set; }
        public short SlHit { get; set; }
        public short SlDefence { get; set; }
        public short SlHP { get; set; }
        public short DarkElement { get; set; }
        public short LightElement { get; set; }
        public short WaterElement { get; set; }
        public short FireElement { get; set; }
        public short ItemVNum { get; set; }

    }
}
