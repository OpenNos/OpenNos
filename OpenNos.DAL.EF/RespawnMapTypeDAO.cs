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
    public class RespawnMapTypeDAO : MappingBaseDAO<RespawnMapType, RespawnMapTypeDTO>, IRespawnMapTypeDAO
    {
        #region Methods

        public void Insert(List<RespawnMapTypeDTO> respawnMapType)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (RespawnMapTypeDTO RespawnMapType in respawnMapType)
                    {
                        RespawnMapType entity = _mapper.Map<RespawnMapType>(RespawnMapType);
                        context.RespawnMapType.Add(entity);
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

        public SaveResult InsertOrUpdate(ref RespawnMapTypeDTO respawnMapType)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    short mapId = respawnMapType.DefaultMapId;
                    RespawnMapType entity = context.RespawnMapType.FirstOrDefault(c => c.DefaultMapId.Equals(mapId));

                    if (entity == null)
                    {
                        respawnMapType = Insert(respawnMapType, context);
                        return SaveResult.Inserted;
                    }

                    respawnMapType.RespawnMapTypeId = entity.RespawnMapTypeId;
                    respawnMapType = Update(entity, respawnMapType, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public RespawnMapTypeDTO LoadById(long respawnMapTypeId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<RespawnMapTypeDTO>(context.RespawnMapType.FirstOrDefault(s => s.RespawnMapTypeId.Equals(respawnMapTypeId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public RespawnMapTypeDTO LoadByMapId(short mapId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<RespawnMapTypeDTO>(context.RespawnMapType.FirstOrDefault(s => s.DefaultMapId.Equals(mapId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private RespawnMapTypeDTO Insert(RespawnMapTypeDTO respawnMapType, OpenNosContext context)
        {
            try
            {
                RespawnMapType entity = _mapper.Map<RespawnMapType>(respawnMapType);
                context.RespawnMapType.Add(entity);
                context.SaveChanges();
                return _mapper.Map<RespawnMapTypeDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private RespawnMapTypeDTO Update(RespawnMapType entity, RespawnMapTypeDTO respawnMapType, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(respawnMapType, entity);
                context.SaveChanges();
            }
            return _mapper.Map<RespawnMapTypeDTO>(entity);
        }

        #endregion
    }
}