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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class RespawnDAO : IRespawnDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnDTO respawn)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                long characterId = respawn.CharacterId;
                short respawnType = respawn.RespawnType;
                Respawn entity = context.respawn.SingleOrDefault(c => c.RespawnType.Equals(respawnType) && c.CharacterId.Equals(characterId));

                if (entity == null) //new entity
                {
                    respawn = Insert(respawn, context);
                    return SaveResult.Inserted;
                }
                else //existing entity
                {
                    respawn.RespawnId = entity.RespawnId;
                    respawn = Update(entity, respawn, context);
                    return SaveResult.Updated;
                }
            }
        }

        public IEnumerable<RespawnDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Respawn respawnobject in context.respawn.Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return Mapper.Map<RespawnDTO>(respawnobject);
                }
            }
        }

        public RespawnDTO LoadById(long respawnId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<RespawnDTO>(context.respawn.FirstOrDefault(s => s.RespawnId.Equals(respawnId)));
            }
        }

        private RespawnDTO Insert(RespawnDTO respawn, OpenNosContainer context)
        {
            Respawn entity = new Respawn() { CharacterId = respawn.CharacterId };
            context.respawn.Add(entity);
            context.SaveChanges();
            return Mapper.Map<RespawnDTO>(entity);
        }

        private RespawnDTO Update(Respawn entity, RespawnDTO respawn, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.respawn.SingleOrDefault(c => c.RespawnId == respawn.RespawnId);
                if (result != null)
                {
                    result = Mapper.Map<RespawnDTO, Respawn>(respawn, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<RespawnDTO>(entity);
        }

        #endregion
    }
}