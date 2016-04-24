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

        public DeleteResult Delete(long characterId, short skillVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                CharacterSkill invItem = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVNum);
                if (invItem != null)
                {
                    context.CharacterSkill.Remove(invItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterSkillDTO characterSkills)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    CharacterSkillDTO CharacterSkill = characterSkills;
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

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterSkill Inventoryobject in context.CharacterSkill.Where(i => i.CharacterId == characterId))
                {
                    yield return Mapper.DynamicMap<CharacterSkillDTO>(Inventoryobject);
                }
            }
        }

        private CharacterSkillDTO Insert(CharacterSkillDTO characterSkill, OpenNosContext context)
        {
            CharacterSkill entity = Mapper.DynamicMap<CharacterSkill>(characterSkill);
            context.CharacterSkill.Add(entity);
            context.SaveChanges();
            return Mapper.DynamicMap<CharacterSkillDTO>(entity);
        }

        private CharacterSkillDTO Update(CharacterSkill entity, CharacterSkillDTO characterSkill, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.DynamicMap(characterSkill, entity);
                context.SaveChanges();
            }

            return Mapper.DynamicMap<CharacterSkillDTO>(characterSkill);
        }

        #endregion
    }
}