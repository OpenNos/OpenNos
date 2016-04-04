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
                CharacterSkill invitem = context.characterskill.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVNum);
                if (invitem != null)
                {
                    context.characterskill.Remove(invitem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterSkillDTO characterskill)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long EntryId = characterskill.CharacterSkillId;
                    CharacterSkill entity = context.characterskill.FirstOrDefault(c => c.CharacterSkillId == EntryId);
                    if (entity == null) //new entity
                    {
                        characterskill = Insert(characterskill, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        entity.CharacterSkillId = context.characterskill.FirstOrDefault(c => c.CharacterSkillId == EntryId).CharacterSkillId;
                        characterskill = Update(entity, characterskill, context);
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
        private CharacterSkillDTO Insert(CharacterSkillDTO characterskill, OpenNosContainer context)
        {

            CharacterSkill entity = Mapper.Map<CharacterSkill>(characterskill);
            context.characterskill.Add(entity);
            context.SaveChanges();
            return Mapper.Map<CharacterSkillDTO>(entity);
        }

        private CharacterSkillDTO Update(CharacterSkill entity, CharacterSkillDTO characterskill, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.characterskill.FirstOrDefault(c => c.CharacterSkillId == characterskill.CharacterSkillId);
                if (result != null)
                {
                    result = Mapper.Map<CharacterSkillDTO, CharacterSkill>(characterskill, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<CharacterSkillDTO>(characterskill);
        }


        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterSkill inventoryobject in context.characterskill.Where(i => i.CharacterId == characterId))
                {
                    yield return Mapper.Map<CharacterSkillDTO>(inventoryobject);
                }
            }
        }

        #endregion
    }
}