using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class ItemDTO
    {
        public short VNum { get; set; }
        public long Price { get; set; }
        public string Name { get; set; }
        public byte Inventory { get; set; }
        public byte ItemType { get; set; }
        public byte EqSlot { get; set; }
        public byte Morph { get; set; }
        public byte Type { get; set; }
        public byte Classe { get; set; }
        public byte Blocked { get; set; }
        public byte Droppable { get; set; }
        public byte Transaction { get; set; }
        public byte Soldable { get; set; }
        public byte MinilandObject { get; set; }
        public byte isWareHouse { get; set; }
        public short LvlMin { get; set; }
        public short DamageMin { get; set; }
        public short DamageMax { get; set; }
        public short Concentrate { get; set; }
        public short HitRate { get; set; }
        public short CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public short RangeDef { get; set; }
        public short DistanceDef { get; set; }
        public short MagicDef { get; set; }
        public string Dodge { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        public short MaxCellon { get; set; }
        public short MaxCellonLvl { get; set; }
        public short FireRez { get; set; }
        public short EauRez { get; set; }
        public short LightRez { get; set; }
        public short DarkRez { get; set; }
        public short DarkElement { get; set; }
        public short LightElement { get; set; }
        public short FireElement { get; set; }
        public short WaterElement { get; set; }
        public short PvpStrength { get; set; }
        public short Speed { get; set; }
        public short Element { get; set; }
        public short ElementRate { get; set; }
        public short PvpDef { get; set; }
        public short DimOposantRez { get; set; }
        public string HpRegen { get; set; }
        public string MpRegen { get; set; }
        public short MoreHp { get; set; }
        public short MoreMp { get; set; }
        public bool Colored { get; set; }
        public bool isConsumable { get; set; }
    }
}
