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
using OpenNos.Data.Enums;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class MinilandObjectDAO : MappingBaseDAO<MinilandObject, MinilandObjectDTO>, IMinilandObjectDAO
    {
        #region Methods

        public DeleteResult DeleteById(long id)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    MinilandObject item = context.MinilandObject.First(i => i.MinilandObjectId.Equals(id));

                    if (item != null)
                    {
                        context.MinilandObject.Remove(item);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref MinilandObjectDTO obj)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long id = obj.MinilandObjectId;
                    MinilandObject entity = context.MinilandObject.FirstOrDefault(c => c.MinilandObjectId.Equals(id));

                    if (entity == null)
                    {
                        obj = Insert(obj, context);
                        return SaveResult.Inserted;
                    }

                    obj.MinilandObjectId = entity.MinilandObjectId;
                    obj = Update(entity, obj, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MinilandObjectDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MinilandObject obj in context.MinilandObject.Where(s => s.CharacterId == characterId))
                {
                    yield return _mapper.Map<MinilandObjectDTO>(obj);
                }
            }
        }

        private MinilandObjectDTO Insert(MinilandObjectDTO obj, OpenNosContext context)
        {
            try
            {
                MinilandObject entity = _mapper.Map<MinilandObject>(obj);
                context.MinilandObject.Add(entity);
                context.SaveChanges();
                return _mapper.Map<MinilandObjectDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private MinilandObjectDTO Update(MinilandObject entity, MinilandObjectDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(respawn, entity);
                context.SaveChanges();
            }
            return _mapper.Map<MinilandObjectDTO>(entity);
        }

        #endregion
    }
}