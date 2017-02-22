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
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class MateDAO : MappingBaseDAO<Mate, MateDTO>, IMateDAO
    {
        #region Methods

        public DeleteResult Delete(long id)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Mate mate = context.Mate.FirstOrDefault(c => c.MateId.Equals(id));
                    if (mate != null)
                    {
                        context.Mate.Remove(mate);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_MATE_ERROR"), e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref MateDTO mate)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long MateId = mate.MateId;
                    Mate entity = context.Mate.FirstOrDefault(c => c.MateId.Equals(MateId));

                    if (entity == null)
                    {
                        mate = Insert(mate, context);
                        return SaveResult.Inserted;
                    }

                    mate = Update(entity, mate, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("INSERT_ERROR"), mate, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MateDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Mate mate in context.Mate.Where(s => s.CharacterId == characterId))
                {
                    yield return _mapper.Map<MateDTO>(mate);
                }
            }
        }

        private MateDTO Insert(MateDTO mate, OpenNosContext context)
        {
            Mate entity = _mapper.Map<Mate>(mate);
            context.Mate.Add(entity);
            context.SaveChanges();
            return _mapper.Map<MateDTO>(entity);
        }

        private MateDTO Update(Mate entity, MateDTO character, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(character, entity);
                context.SaveChanges();
            }

            return _mapper.Map<MateDTO>(entity);
        }

        #endregion
    }
}