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

using AutoMapper;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class NpcMonster : NpcMonsterDTO
    {
        #region Instantiation

        public NpcMonster(short NpcMonsterVNum)
        {
            Mapper.CreateMap<NpcMonsterDTO, NpcMonster>();
            Mapper.CreateMap<NpcMonster, NpcMonsterDTO>();
            Teleporters = DAOFactory.TeleporterDAO.LoadFromNpc(NpcMonsterVNum);
            Drops = DAOFactory.DropDAO.LoadByMonster(NpcMonsterVNum);
            LastEffect = LastMove = DateTime.Now;
            Skills = Mapper.DynamicMap<IEnumerable<NpcMonsterSkill>>(DAOFactory.NpcMonsterSkillDAO.LoadByNpcMonster(NpcMonsterVNum));
        }

        #endregion

        #region Properties

        public IEnumerable<DropDTO> Drops { get; set; }
        public short firstX { get; set; }
        public short firstY { get; set; }
        public DateTime LastEffect { get; private set; }
        public DateTime LastMove { get; private set; }
        public IEnumerable<NpcMonsterSkill> Skills { get; set; }
        public IEnumerable<TeleporterDTO> Teleporters { get; set; }

        #endregion

        #region Methods

        public string GenerateEInfo()
        {
            return $"e_info 10 {NpcMonsterVNum} {Level} {Element} {AttackClass} {ElementRate} {AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {CriticalLuckRate} {CriticalRate} {DefenceUpgrade} {CloseDefence} {DefenceDodge} {DistanceDefence} {DistanceDefenceDodge} {MagicDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance} {MaxHP} {MaxMP} -1 {Name.Replace(' ', '^')}"; // {Hp} {Mp} in 0 0
        }

        #endregion
    }
}