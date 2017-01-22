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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class FamilyDAO : MappingBaseDAO<Family, FamilyDTO>, IFamilyDAO
    {
        #region Methods

        public DeleteResult Delete(long familyId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Family Fam = context.Family.FirstOrDefault(c => c.FamilyId == familyId);

                    if (Fam != null)
                    {
                        context.Family.Remove(Fam);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), familyId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref FamilyDTO family)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long AccountId = family.FamilyId;
                    Family entity = context.Family.FirstOrDefault(c => c.FamilyId.Equals(AccountId));

                    if (entity == null)
                    {
                        family = Insert(family, context);
                        return SaveResult.Inserted;
                    }

                    family = Update(entity, family, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_FAMILY_ERROR"), family.FamilyId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<FamilyDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Family entity in context.Family)
                {
                    yield return _mapper.Map<FamilyDTO>(entity);
                }
            }
        }

        public FamilyDTO LoadByCharacterId(long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    FamilyCharacter familyCharacter = context.FamilyCharacter.FirstOrDefault(fc => fc.Character.CharacterId.Equals(characterId));
                    if (familyCharacter != null)
                    {
                        Family family = context.Family.FirstOrDefault(a => a.FamilyId.Equals(familyCharacter.FamilyId));
                        if (family != null)
                        {
                            return _mapper.Map<FamilyDTO>(family);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public FamilyDTO LoadById(long familyId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Family family = context.Family.FirstOrDefault(a => a.FamilyId.Equals(familyId));
                    if (family != null)
                    {
                        return _mapper.Map<FamilyDTO>(family);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public FamilyDTO LoadByName(string name)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Family family = context.Family.FirstOrDefault(a => a.Name.Equals(name));
                    if (family != null)
                    {
                        return _mapper.Map<FamilyDTO>(family);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        private FamilyDTO Insert(FamilyDTO family, OpenNosContext context)
        {
            Family entity = _mapper.Map<Family>(family);
            context.Family.Add(entity);
            context.SaveChanges();
            return _mapper.Map<FamilyDTO>(entity);
        }

        private FamilyDTO Update(Family entity, FamilyDTO family, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(family, entity);
                context.SaveChanges();
            }
            return _mapper.Map<FamilyDTO>(entity);
        }

        #endregion
    }
}