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

using OpenNos.Core;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class NpcMonsterSkillDAO : MappingBaseDAO<NpcMonsterSkill, NpcMonsterSkillDTO>, INpcMonsterSkillDAO
    {
        #region Methods

        public NpcMonsterSkillDTO Insert(ref NpcMonsterSkillDTO npcMonsterskill)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    NpcMonsterSkill entity = _mapper.Map<NpcMonsterSkill>(npcMonsterskill);
                    context.NpcMonsterSkill.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<NpcMonsterSkillDTO>(entity);
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
                    foreach (NpcMonsterSkillDTO Skill in skills)
                    {
                        NpcMonsterSkill entity = _mapper.Map<NpcMonsterSkill>(Skill);
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
                return context.NpcMonsterSkill.ToList().Select(n => _mapper.Map<NpcMonsterSkillDTO>(n)).ToList();
            }
        }

        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonsterSkill NpcMonsterSkillobject in context.NpcMonsterSkill.Where(i => i.NpcMonsterVNum == npcId))
                {
                    yield return _mapper.Map<NpcMonsterSkillDTO>(NpcMonsterSkillobject);
                }
            }
        }

        #endregion
    }
}