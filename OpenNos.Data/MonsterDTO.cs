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
    public class MonsterDTO
    {
        #region Properties

        public short MonsterVNum { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string AttackClass { get; set; }
        public string AttackUpgrade { get; set; }
        public short DamageMinimum { get; set; }
        public short DamageMaximum { get; set; }
        public short HitRate { get; set; }
        public string Concentrate { get; set; }
        public string CriticalLuck { get; set; }
        public string CriticalLuckRate { get; set; }
        public string Element { get; set; }
        public string ElementRate { get; set; }
        public string MaxHP { get; set; }
        public string MaxMP { get; set; }
        public string CloseDefence { get; set; }
        public string DistanceDefence { get; set; }
        public string MagicDefence { get; set; }
        public short DefenceUpgrade { get; set; }
        public short DefenceDodge { get; set; }
        public short DistanceDefenceDodge { get; set; }
        public short FireResistance { get; set; }
        public short WaterResistance { get; set; }
        public short LightResistance { get; set; }
        public short DarkResistance { get; set; }
        public short Speed { get; set; }

        #endregion
    }
}
