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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class NpcMonsterSkillDao : MappingBaseDao<NpcMonsterSkill, NpcMonsterSkillDTO>, INpcMonsterSkillDAO
    {
        #region Methods

        public NpcMonsterSkillDTO Insert(ref NpcMonsterSkillDTO npcMonsterskill)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    NpcMonsterSkill entity = Mapper.Map<NpcMonsterSkill>(npcMonsterskill);
                    context.NpcMonsterSkill.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<NpcMonsterSkillDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void Insert(List<NpcMonsterSkillDTO> skills)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (NpcMonsterSkillDTO skill in skills)
                    {
                        NpcMonsterSkill entity = Mapper.Map<NpcMonsterSkill>(skill);
                        context.NpcMonsterSkill.Add(entity);
                    }
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public List<NpcMonsterSkillDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return context.NpcMonsterSkill.ToList().Select(n => Mapper.Map<NpcMonsterSkillDTO>(n)).ToList();
            }
        }

        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonsterSkill npcMonsterSkillobject in context.NpcMonsterSkill.Where(i => i.NpcMonsterVNum == npcId))
                {
                    yield return Mapper.Map<NpcMonsterSkillDTO>(npcMonsterSkillobject);
                }
            }
        }

        #endregion
    }
}