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
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public CharacterSkillDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CharacterSkill, CharacterSkillDTO>();
                cfg.CreateMap<CharacterSkillDTO, CharacterSkill>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

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

        public IEnumerable<CharacterSkillDTO> InsertOrUpdate(IEnumerable<CharacterSkillDTO> characterSkills)
        {
            try
            {
                List<CharacterSkillDTO> returnSkills = new List<CharacterSkillDTO>();

                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach(CharacterSkillDTO skill in characterSkills)
                    {
                        CharacterSkillDTO returnSkill = skill;
                        SaveResult result = InsertOrUpdate(ref returnSkill, context);
                        returnSkills.Add(returnSkill);
                    }
                }

                return returnSkills;
            }
            catch (Exception e)
            {
                Logger.Log.Error(String.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return Enumerable.Empty<CharacterSkillDTO>();
            }
        }

        public SaveResult InsertOrUpdate(ref CharacterSkillDTO characterSkill)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return InsertOrUpdate(ref characterSkill, context);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(String.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return SaveResult.Error;
            }
        }

        private SaveResult InsertOrUpdate(ref CharacterSkillDTO characterSkill, OpenNosContext context)
        {
            long characterId = characterSkill.CharacterId;
            long skillVnum = characterSkill.SkillVNum;
            CharacterSkill entity = context.CharacterSkill.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVnum);
            if (entity == null) //new entity
            {
                characterSkill = Insert(characterSkill, context);
                return SaveResult.Inserted;
            }
            else //existing entity
            {
                 characterSkill = Update(entity, characterSkill, context);
                return SaveResult.Updated;
            }
        }

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterSkill Inventoryobject in context.CharacterSkill.Where(i => i.CharacterId == characterId))
                {
                    yield return _mapper.Map<CharacterSkillDTO>(Inventoryobject);
                }
            }
        }

        private CharacterSkillDTO Insert(CharacterSkillDTO characterSkill, OpenNosContext context)
        {
            CharacterSkill entity = _mapper.Map<CharacterSkill>(characterSkill);
            context.CharacterSkill.Add(entity);
            context.SaveChanges();
            return _mapper.Map<CharacterSkillDTO>(entity);
        }

        private CharacterSkillDTO Update(CharacterSkill entity, CharacterSkillDTO characterSkill, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(characterSkill, entity);
                context.SaveChanges();
            }

            return _mapper.Map<CharacterSkillDTO>(characterSkill);
        }

        #endregion
    }
}