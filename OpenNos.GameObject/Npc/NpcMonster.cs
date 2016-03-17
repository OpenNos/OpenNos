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
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class NpcMonster : NpcMonsterDTO
    {
        #region Instantiation

        public short firstX
        {
            get; set;
        }
        public short firstY
        {
            get; set;
        }
        public NpcMonster()
        {
            Mapper.CreateMap<NpcMonsterDTO, NpcMonster>();
            Mapper.CreateMap<NpcMonster, NpcMonsterDTO>();

        }
        public NpcMonster(short npcId)
        {
            Mapper.CreateMap<NpcMonsterDTO, NpcMonster>();
            Mapper.CreateMap<NpcMonster, NpcMonsterDTO>();

            LastEffect = LastMove = DateTime.Now;

            IEnumerable<TeleporterDTO> Teleporters = DAOFactory.TeleporterDAO.LoadFromNpc(NpcMonsterVNum);
         }

        public string GenerateEInfo()
        {
            return $"e_info 10 {NpcMonsterVNum} {Level} {Element} {AttackClass} {ElementRate} {AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {CriticalLuckRate} {CriticalRate} {DefenceUpgrade} {CloseDefence} {DefenceDodge} {DistanceDefence} {DistanceDefenceDodge} {MagicDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance} 0 0 -1 {Name.Replace(' ', '^')}"; // {Hp} {Mp} in 0 0 

        }

        #endregion

        #region Properties
        public IEnumerable<TeleporterDTO> Teleporters { get; set; }
        public DateTime LastMove { get; private set; }
        public DateTime LastEffect { get; private set; }
        #endregion

        #region Methods
        

        #endregion
    }
}