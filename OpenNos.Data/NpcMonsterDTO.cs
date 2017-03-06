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

using OpenNos.Domain;

namespace OpenNos.Data
{
    public class NpcMonsterDTO : MappingBaseDTO
    {
        #region Properties

        public byte AmountRequired { get; set; }

        public byte AttackClass { get; set; }

        public byte AttackUpgrade { get; set; }

        public byte BasicArea { get; set; }

        public short BasicCooldown { get; set; }

        public byte BasicRange { get; set; }

        public short BasicSkill { get; set; }

        public short CloseDefence { get; set; }

        public short Concentrate { get; set; }

        public byte CriticalChance { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public sbyte DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public byte DefenceUpgrade { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        public byte Element { get; set; }

        public short ElementRate { get; set; }

        public sbyte FireResistance { get; set; }

        public byte HeroLevel { get; set; }

        public int HeroXp { get; set; }

        public bool IsHostile { get; set; }

        public int JobXP { get; set; }

        public byte Level { get; set; }

        public sbyte LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public int MaxHP { get; set; }

        public int MaxMP { get; set; }

        public MonsterType MonsterType { get; set; }

        public string Name { get; set; }

        public bool NoAggresiveIcon { get; set; }

        public byte NoticeRange { get; set; }

        public short NpcMonsterVNum { get; set; }

        public byte Race { get; set; }

        public byte RaceType { get; set; }

        public int RespawnTime { get; set; }

        public byte Speed { get; set; }

        public short VNumRequired { get; set; }

        public sbyte WaterResistance { get; set; }

        public int XP { get; set; }

        #endregion
    }
}