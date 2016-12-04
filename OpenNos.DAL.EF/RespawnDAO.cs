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
    public class RespawnDao : MappingBaseDao<Respawn, RespawnDTO>, IRespawnDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnDTO respawn)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long characterId = respawn.CharacterId;
                    long respawnMapTypeId = respawn.RespawnMapTypeId;
                    Respawn entity = context.Respawn.FirstOrDefault(c => c.RespawnMapTypeId.Equals(respawnMapTypeId) && c.CharacterId.Equals(characterId));

                    if (entity == null)
                    {
                        respawn = Insert(respawn, context);
                        return SaveResult.Inserted;
                    }
                    respawn.RespawnId = entity.RespawnId;
                    respawn = Update(entity, respawn, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<RespawnDTO> LoadByCharacter(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Respawn respawnobject in context.Respawn.Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return Mapper.Map<RespawnDTO>(respawnobject);
                }
            }
        }

        public RespawnDTO LoadById(long respawnId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return Mapper.Map<RespawnDTO>(context.Respawn.FirstOrDefault(s => s.RespawnId.Equals(respawnId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private RespawnDTO Insert(RespawnDTO respawn, OpenNosContext context)
        {
            try
            {
                Respawn entity = Mapper.Map<Respawn>(respawn);
                context.Respawn.Add(entity);
                context.SaveChanges();
                return Mapper.Map<RespawnDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private RespawnDTO Update(Respawn entity, RespawnDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Map(respawn, entity);
                context.SaveChanges();
            }
            return Mapper.Map<RespawnDTO>(entity);
        }

        #endregion
    }
}