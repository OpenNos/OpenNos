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

using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class NpcMonster : NpcMonsterDTO
    {
        #region Properties

        public List<DropDTO> Drops { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public List<BCard> BCards { get; set; }

        public DateTime LastEffect { get; private set; }

        public DateTime LastMove { get; private set; }

        public List<NpcMonsterSkill> Skills { get; set; }

        public List<TeleporterDTO> Teleporters { get; set; }

        #endregion

        #region Methods

        public string GenerateEInfo()
        {
            return $"e_info 10 {NpcMonsterVNum} {Level} {Element} {AttackClass} {ElementRate} {AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {CriticalChance} {CriticalRate} {DefenceUpgrade} {CloseDefence} {DefenceDodge} {DistanceDefence} {DistanceDefenceDodge} {MagicDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance} {MaxHP} {MaxMP} -1 {Name.Replace(' ', '^')}";
        }

        public float GetRes(int skillelement)
        {
            switch (skillelement)
            {
                case 0:
                    return FireResistance / 100;

                case 1:
                    return WaterResistance / 100;

                case 2:
                    return LightResistance / 100;

                case 3:
                    return DarkResistance / 100;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Intializes the GameObject, will be injected by AutoMapper after Entity -&gt; GO mapping
        /// </summary>
        public override void Initialize()
        {
            Teleporters = ServerManager.Instance.GetTeleportersByNpcVNum(NpcMonsterVNum);
            Drops = ServerManager.Instance.GetDropsByMonsterVNum(NpcMonsterVNum);
            LastEffect = LastMove = DateTime.Now;
            Skills = ServerManager.Instance.GetNpcMonsterSkillsByMonsterVNum(NpcMonsterVNum);
        }

        #endregion
    }
}