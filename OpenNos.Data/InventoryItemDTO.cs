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
        public short CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public short RangeDefence { get; set; }
        public short DistanceDefence { get; set; }
        public short MagicDefence { get; set; }
        public short DistanceDefenceDodge { get; set; }
        public short DefenceDodge { get; set; }
        public short ElementRate { get; set; }
        public short Upgrade { get; set; }
        public short Rare { get; set; }
        public short Color { get; set; }
        public short Amount { get; set; }
        public short SpLevel { get; set; }
        public short SpXp { get; set; }
        public short SlElement { get; set; }
        public short SlHit { get; set; }
        public short SlDefence { get; set; }
        public short SlHP { get; set; }
        public short DarkElement { get; set; }
        public short LightElement { get; set; }
        public short WaterElement { get; set; }
        public short FireElement { get; set; }
        public short ItemVNum { get; set; }
        public short Ammo { get; set; }
        public bool IsFixed { get; set; }

        #endregion
    }
}