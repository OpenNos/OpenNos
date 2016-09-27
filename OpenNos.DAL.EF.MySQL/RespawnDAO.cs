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
    public class RespawnDAO : IRespawnDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public RespawnDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Respawn, RespawnDTO>();
                cfg.CreateMap<RespawnDTO, Respawn>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnDTO respawn)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long CharacterId = respawn.CharacterId;
                    short RespawnType = respawn.RespawnType;
                    Respawn entity = context.Respawn.FirstOrDefault(c => c.RespawnType.Equals(RespawnType) && c.CharacterId.Equals(CharacterId));

                    if (entity == null)
                    {
                        respawn = Insert(respawn, context);
                        return SaveResult.Inserted;
                    }
                    else
                    {
                        respawn.RespawnId = entity.RespawnId;
                        respawn = Update(entity, respawn, context);
                        return SaveResult.Updated;
                    }
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
                foreach (Respawn Respawnobject in context.Respawn.Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return _mapper.Map<RespawnDTO>(Respawnobject);
                }
            }
        }

        public RespawnDTO LoadById(long respawnId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<RespawnDTO>(context.Respawn.FirstOrDefault(s => s.RespawnId.Equals(respawnId)));
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
                Respawn entity = new Respawn() { CharacterId = respawn.CharacterId };
                context.Respawn.Add(entity);
                context.SaveChanges();
                return _mapper.Map<RespawnDTO>(entity);
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
                _mapper.Map(respawn, entity);
                context.SaveChanges();
            }
            return _mapper.Map<RespawnDTO>(entity);
        }

        #endregion
    }
}