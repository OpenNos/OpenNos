/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;

namespace OpenNos.Data
{
    public class ItemDTO
    {
        #region Properties

        public short VNum { get; set; }
        public long Price { get; set; }
        public string Name { get; set; }
        public byte ItemType { get; set; }
        public byte EquipmentSlot { get; set; }
        public short Morph { get; set; }
        public byte Type { get; set; }
        public byte Class { get; set; }
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
        public short DistanceDefenceDodge { get; set; }
        public short DefenceDodge { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        public short LevelJobMinimum { get; set; }
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
        public short ReduceOposantResistance { get; set; }
        public short HpRegeneration { get; set; }
        public short MpRegeneration { get; set; }
        public short MoreHp { get; set; }
        public short MoreMp { get; set; }
        public bool Colored { get; set; }
        public bool isConsumable { get; set; }
        public byte ReputationMinimum { get; set; }
        public short FairyMaximumLevel { get; set; }
        public short MaximumAmmo { get; set; }
        public short BasicUpgrade { get; set; }
        public short FashionType { get; set; }
        public long ItemValidTime { get; set; }
        public bool isPearl { get; set; }
        public short Color { get; set; }
        public short Effect { get; set; }
        public short Value { get; set; }
        #endregion
    }
}