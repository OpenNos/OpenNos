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
        public byte EquipmentSlot { get; set; }
        public short Morph { get; set; }
        public byte Type { get; set; }
        public byte Classe { get; set; }
        public byte Blocked { get; set; }
        public byte Droppable { get; set; }
        public byte Transaction { get; set; }
        public byte Soldable { get; set; }
        public byte MinilandObject { get; set; }
        public byte isWareHouse { get; set; }
        public short LevelMinimum { get; set; }
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
        public short LevelJobMinimum { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        public short MaxCellon { get; set; }
        public short MaxCellonLvl { get; set; }
        public short FireResistance { get; set; }
        public short WaterResistance { get; set; }
        public short LightResistance { get; set; }
        public short DarkResistance { get; set; }
        public short DarkElement { get; set; }
        public short LightElement { get; set; }
        public short FireElement { get; set; }
        public short WaterElement { get; set; }
        public short PvpStrength { get; set; }
        public short Speed { get; set; }
        public short Element { get; set; }
        public short ElementRate { get; set; }
        public short PvpDefence { get; set; }
        public short DimOposantResistance { get; set; }
        public string HpRegeneration { get; set; }
        public string MpRegeneration { get; set; }
        public short MoreHp { get; set; }
        public short MoreMp { get; set; }
        public bool Colored { get; set; }
        public bool isConsumable { get; set; }
        public long ReputationMinimum { get; set; }
        public short FairyMaximumLevel { get; set; }
        public short MaximumAmmo { get; set; }
    }
}
