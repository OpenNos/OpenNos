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
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class RespawnDAO : IRespawnDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnDTO Respawn)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                long CharacterId = Respawn.CharacterId;
                short RespawnType = Respawn.RespawnType;
                Respawn entity = context.Respawn.FirstOrDefault(c => c.RespawnType.Equals(RespawnType) && c.CharacterId.Equals(CharacterId));

                if (entity == null) //new entity
                {
                    Respawn = Insert(Respawn, context);
                    return SaveResult.Inserted;
                }
                else //existing entity
                {
                    Respawn.RespawnId = entity.RespawnId;
                    Respawn = Update(entity, Respawn, context);
                    return SaveResult.Updated;
                }
            }
        }

        public IEnumerable<RespawnDTO> LoadByCharacterId(long CharacterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Respawn Respawnobject in context.Respawn.Where(i => i.CharacterId.Equals(CharacterId)))
                {
                    yield return Mapper.DynamicMap<RespawnDTO>(Respawnobject);
                }
            }
        }

        public RespawnDTO LoadById(long RespawnId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<RespawnDTO>(context.Respawn.FirstOrDefault(s => s.RespawnId.Equals(RespawnId)));
            }
        }

        private RespawnDTO Insert(RespawnDTO Respawn, OpenNosContext context)
        {
            Respawn entity = new Respawn() { CharacterId = Respawn.CharacterId };
            context.Respawn.Add(entity);
            context.SaveChanges();
            return Mapper.DynamicMap<RespawnDTO>(entity);
        }

        private RespawnDTO Update(Respawn entity, RespawnDTO Respawn, OpenNosContext context)
        {
            using (context)
            {
                var result = context.Respawn.FirstOrDefault(c => c.RespawnId == Respawn.RespawnId);
                if (result != null)
                {
                    result = Mapper.Map<RespawnDTO, Respawn>(Respawn, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.DynamicMap<RespawnDTO>(entity);
        }

        #endregion
    }
}