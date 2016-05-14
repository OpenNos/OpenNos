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

namespace OpenNos.GameObject
{
    public class CharacterSkill : CharacterSkillDTO
    {
        #region Instantiation

        public CharacterSkill(CharacterSkillDTO dto)
        {
            this.CharacterId = dto.CharacterId;
            this.CharacterSkillId = dto.CharacterSkillId;
            this.SkillVNum = dto.SkillVNum;
            this.NpcMonsterVNum = dto.NpcMonsterVNum;
            LastUse = DateTime.Now.AddHours(-1);
            Used = false;
            Hit = 0;
        }

        public CharacterSkill()
        {
            LastUse = DateTime.Now.AddHours(-1);
            Used = false;
            Hit = 0;
        }

        #endregion

        #region Properties

        public short Hit { get; set; }

        public DateTime LastUse
        {
            get; set;
        }

        public bool Used { get; set; }

        #endregion
    }
}