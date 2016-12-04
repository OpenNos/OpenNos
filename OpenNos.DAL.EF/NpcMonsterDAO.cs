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
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class NpcMonsterDao : MappingBaseDao<NpcMonster, NpcMonsterDTO>, INpcMonsterDAO
    {
        #region Methods

        public IEnumerable<NpcMonsterDTO> FindByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonster npcMonster in context.NpcMonster.Where(s => s.Name.Contains(name)))
                {
                    yield return Mapper.Map<NpcMonsterDTO>(npcMonster);
                }
            }
        }

        public SaveResult InsertOrUpdate(ref NpcMonsterDTO npcMonster)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    short npcMonsterVNum = npcMonster.NpcMonsterVNum;
                    NpcMonster entity = context.NpcMonster.FirstOrDefault(c => c.NpcMonsterVNum.Equals(npcMonsterVNum));

                    if (entity == null)
                    {
                        npcMonster = Insert(npcMonster, context);
                        return SaveResult.Inserted;
                    }
                    npcMonster = Update(entity, npcMonster, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_NPCMONSTER_ERROR"), npcMonster.NpcMonsterVNum, e.Message), e);
                return SaveResult.Error;
            }
        }

        public void Insert(List<NpcMonsterDTO> npcs)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (NpcMonsterDTO item in npcs)
                    {
                        NpcMonster entity = Mapper.Map<NpcMonster>(item);
                        context.NpcMonster.Add(entity);
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

        public NpcMonsterDTO Insert(NpcMonsterDTO npc)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    NpcMonster entity = Mapper.Map<NpcMonster>(npc);
                    context.NpcMonster.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<NpcMonsterDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<NpcMonsterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonster npcMonster in context.NpcMonster)
                {
                    yield return Mapper.Map<NpcMonsterDTO>(npcMonster);
                }
            }
        }

        public NpcMonsterDTO LoadByVNum(short vnum)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return Mapper.Map<NpcMonsterDTO>(context.NpcMonster.FirstOrDefault(i => i.NpcMonsterVNum.Equals(vnum)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private NpcMonsterDTO Insert(NpcMonsterDTO npcMonster, OpenNosContext context)
        {
            NpcMonster entity = Mapper.Map<NpcMonster>(npcMonster);
            context.NpcMonster.Add(entity);
            context.SaveChanges();
            return Mapper.Map<NpcMonsterDTO>(entity);
        }

        private NpcMonsterDTO Update(NpcMonster entity, NpcMonsterDTO npcMonster, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Map(npcMonster, entity);
                context.SaveChanges();
            }
            return Mapper.Map<NpcMonsterDTO>(entity);
        }

        #endregion
    }
}