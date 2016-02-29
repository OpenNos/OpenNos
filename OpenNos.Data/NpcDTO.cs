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
    public class NpcDTO
    {
        #region Properties

        public short NpcId { get; set; }
        public string Name { get; set; }
        public short Vnum { get; set; }
        public short Dialog { get; set; }
        public short MapId { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
        public short Position { get; set; }
        public byte Level { get; set; }
        public byte Element { get; set; }
        public byte AttackClass { get; set; }
        public short ElementRate { get; set; }
        public byte AttackUpgrade { get; set; }
        public short DamageMinimum { get; set; }
        public short DamageMaximum { get; set; }
        public short Concentrate { get; set; }
        public short CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public byte DefenceUpgrade { get; set; }
        public short DefenceDodge { get; set; }
        public short DistanceDefence { get; set; }
        public short DistanceDefenceDodge { get; set; }
        public short MagicDefence { get; set; }
        public short CloseDefence { get; set; }
        public short FireElement { get; set; }
        public short WaterElement { get; set; }
        public short LightElement { get; set; }
        public short DarkElement { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        #endregion
    }
}