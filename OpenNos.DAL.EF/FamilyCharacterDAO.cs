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
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class FamilyCharacterDAO : MappingBaseDAO<FamilyCharacter, FamilyCharacterDTO>, IFamilyCharacterDAO
    {
        #region Methods

        public DeleteResult Delete(string characterName)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Character character = context.Character.FirstOrDefault(c => c.Name.Equals(characterName) && c.State == (byte)CharacterState.Active);
                    FamilyCharacter familyCharacter = context.FamilyCharacter.FirstOrDefault(c => c.CharacterId.Equals(character.CharacterId));
                    if (character != null && familyCharacter != null)
                    {
                        context.FamilyCharacter.Remove(familyCharacter);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_FAMILYCHARACTER_ERROR"), e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref FamilyCharacterDTO character)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long familyCharacterId = character.FamilyCharacterId;
                    FamilyCharacter entity = context.FamilyCharacter.FirstOrDefault(c => c.FamilyCharacterId.Equals(familyCharacterId));

                    if (entity == null)
                    {
                        character = Insert(character, context);
                        return SaveResult.Inserted;
                    }

                    character = Update(entity, character, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), character, e.Message), e);
                return SaveResult.Error;
            }
        }

        public FamilyCharacterDTO LoadByCharacterId(long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<FamilyCharacterDTO>(context.FamilyCharacter.FirstOrDefault(c => c.CharacterId == characterId));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IList<FamilyCharacterDTO> LoadByFamilyId(long familyId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return context.FamilyCharacter.Where(fc => fc.FamilyId.Equals(familyId)).ToList().Select(c => _mapper.Map<FamilyCharacterDTO>(c)).ToList();
            }
        }

        public FamilyCharacterDTO LoadById(long familyCharacterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<FamilyCharacterDTO>(context.FamilyCharacter.FirstOrDefault(c => c.FamilyCharacterId.Equals(familyCharacterId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private FamilyCharacterDTO Insert(FamilyCharacterDTO character, OpenNosContext context)
        {
            FamilyCharacter entity = _mapper.Map<FamilyCharacter>(character);
            context.FamilyCharacter.Add(entity);
            context.SaveChanges();
            return _mapper.Map<FamilyCharacterDTO>(entity);
        }

        private FamilyCharacterDTO Update(FamilyCharacter entity, FamilyCharacterDTO character, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(character, entity);
                context.SaveChanges();
            }

            return _mapper.Map<FamilyCharacterDTO>(entity);
        }

        #endregion
    }
}