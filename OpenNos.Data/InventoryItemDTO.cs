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

using System;
namespace OpenNos.Data
{
    public class InventoryItemDTO
    {
        #region Properties

        public long InventoryItemId { get; set; }
        public short DamageMinimum { get; set; }
        public short DamageMaximum { get; set; }
        public short Concentrate { get; set; }
        public short HitRate { get; set; }
        public byte CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public short CloseDefence { get; set; }
        public short DistanceDefence { get; set; }
        public short MagicDefence { get; set; }
        public short DistanceDefenceDodge { get; set; }
        public short DefenceDodge { get; set; }
        public short ElementRate { get; set; }
        public byte Upgrade { get; set; }
        public byte Rare { get; set; }
        public short Design { get; set; }
        public byte Amount { get; set; }
        public byte SpLevel { get; set; }
        public short SpXp { get; set; }
        public short SlElement { get; set; }
        public short SlDamage { get; set; }
        public short HP { get; set; }
        public short MP { get; set; }
        public short SlDefence { get; set; }
        public short SlHP { get; set; }
        public byte DarkElement { get; set; }
        public byte LightElement { get; set; }
        public byte WaterElement { get; set; }
        public byte FireElement { get; set; }
        public short ItemVNum { get; set; }
        public byte Ammo { get; set; }
        public bool IsFixed { get; set; }
        public long ItemValidTime { get; set; }
        public byte Cellon { get; set; }
        public byte FireResistance { get; set; }
        public byte WaterResistance { get; set; }
        public byte LightResistance { get; set; }
        public byte DarkResistance { get; set; }
        public short CriticalDodge { get; set; }
        public bool IsEmpty { get; set; }
        public byte SpDamage { get; set; }
        public byte SpElement { get; set; }
        public byte SpDefence { get; set; }
        public byte SpHP { get; set; }
        public byte SpDark { get; set; }
        public byte SpLight { get; set; }
        public byte SpFire { get; set; }
        public byte SpWater { get; set; }
        public byte SpStoneUpgrade { get; set; }

        #endregion
    }
}