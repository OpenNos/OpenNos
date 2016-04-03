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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class NpcMonsterSkillDAO : INpcMonsterSkillDAO
    {
        #region Methods

        public NpcMonsterSkillDTO Insert(ref NpcMonsterSkillDTO npcmonsterskill)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                NpcMonsterSkill entity = Mapper.Map<NpcMonsterSkill>(npcmonsterskill);
                context.npcmonsterskill.Add(entity);
                context.SaveChanges();
                return Mapper.Map<NpcMonsterSkillDTO>(entity);
            }
        }
        public void Insert(List<NpcMonsterSkillDTO> skills)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (NpcMonsterSkillDTO skill in skills)
                {
                    NpcMonsterSkill entity = Mapper.Map<NpcMonsterSkill>(skill);
                    context.npcmonsterskill.Add(entity);
                }
                context.SaveChanges();

            }
        }
        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonsterSkill npcmonsterskillobject in context.npcmonsterskill.Where(i => i.NpcMonsterVNum == npcId))
                {
                    yield return Mapper.Map<NpcMonsterSkillDTO>(npcmonsterskillobject);
                }
            }
        }

        #endregion
    }
}