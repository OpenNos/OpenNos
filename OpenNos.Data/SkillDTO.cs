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
    public class SkillDTO
    {
        #region Properties

        public int AttackAnimation { get; set; }
        public int CastAnimation { get; set; }
        public int CastEffect { get; set; }
        public int CastId { get; set; }
        public int Cooldown { get; set; }
        public int Cost { get; set; }
        public short Damage { get; set; }
        public short Distance { get; set; }
        public int Duration { get; set; }
        public int Effect { get; set; }
        public byte Element { get; set; }
        public short ElementalDamage { get; set; }
        public int JobLevelMinimum { get; set; }
        public int Level { get; set; }
        public int MinimumAdventurerLevel { get; set; }
        public int MinimumArcherLevel { get; set; }
        public int MinimumMagicianLevel { get; set; }
        public int MinimumSwordmanLevel { get; set; }
        public int MpCost { get; set; }
        public byte CPCost { get; set; }
        public string Name { get; set; }
        public int Range { get; set; }
        public short SkillVNum { get; set; }
        public short Type { get; set; }

        #endregion
    }
}