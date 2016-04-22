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
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class CharacterSkillDAO : ICharacterSkillDAO
    {
        #region Methods

        public DeleteResult Delete(long CharacterId, short SkillVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                CharacterSkill invItem = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == CharacterId && i.SkillVNum == SkillVNum);
                if (invItem != null)
                {
                    context.CharacterSkill.Remove(invItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterSkillDTO CharacterSkills)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    CharacterSkillDTO CharacterSkill = CharacterSkills;
                    CharacterSkill entity = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == CharacterSkill.CharacterId && i.SkillVNum == CharacterSkill.SkillVNum);
                    if (entity == null) //new entity
                    {
                        CharacterSkill = Insert(CharacterSkill, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        entity.CharacterSkillId = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == CharacterSkill.CharacterId && i.SkillVNum == CharacterSkill.SkillVNum).CharacterSkillId;
                        CharacterSkill = Update(entity, CharacterSkill, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message);
                return SaveResult.Error;
            }
        }
        private CharacterSkillDTO Insert(CharacterSkillDTO CharacterSkill, OpenNosContext context)
        {

            CharacterSkill entity = Mapper.DynamicMap<CharacterSkill>(CharacterSkill);
            context.CharacterSkill.Add(entity);
            context.SaveChanges();
            return Mapper.DynamicMap<CharacterSkillDTO>(entity);
        }

        private CharacterSkillDTO Update(CharacterSkill entity, CharacterSkillDTO CharacterSkill, OpenNosContext context)
        {
            using (context)
            {
                var result = context.CharacterSkill.FirstOrDefault(c => c.CharacterSkillId == CharacterSkill.CharacterSkillId);
                if (result != null)
                {
                    result = Mapper.Map<CharacterSkillDTO, CharacterSkill>(CharacterSkill, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.DynamicMap<CharacterSkillDTO>(CharacterSkill);
        }


        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long CharacterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterSkill Inventoryobject in context.CharacterSkill.Where(i => i.CharacterId == CharacterId))
                {
                    yield return Mapper.DynamicMap<CharacterSkillDTO>(Inventoryobject);
                }
            }
        }

        #endregion
    }
}