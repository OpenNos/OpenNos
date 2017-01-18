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
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using OpenNos.DAL.EF.DB;

namespace OpenNos.DAL.EF
{
    public class StaticBonusDAO : MappingBaseDAO<StaticBonus, StaticBonusDTO>, IStaticBonusDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref StaticBonusDTO staticBonus)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long id = staticBonus.StaticBonusId;
                    StaticBonus entity = context.StaticBonus.FirstOrDefault(c => c.StaticBonusId.Equals(id));

                    if (entity == null)
                    {
                        staticBonus = Insert(staticBonus, context);
                        return SaveResult.Inserted;
                    }
                    staticBonus.StaticBonusId = entity.StaticBonusId;
                    staticBonus = Update(entity, staticBonus, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public StaticBonusDTO LoadById(long sbId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<StaticBonusDTO>(context.RespawnMapType.FirstOrDefault(s => s.RespawnMapTypeId.Equals(sbId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
        

        private StaticBonusDTO Insert(StaticBonusDTO sb, OpenNosContext context)
        {
            try
            {
                StaticBonus entity = _mapper.Map<StaticBonus>(sb);
                context.StaticBonus.Add(entity);
                context.SaveChanges();
                return _mapper.Map<StaticBonusDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private StaticBonusDTO Update(StaticBonus entity, StaticBonusDTO sb, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(sb, entity);
                context.SaveChanges();
            }
            return _mapper.Map<StaticBonusDTO>(entity);
        }

        public void RemoveOutDated()
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (StaticBonus entity in context.StaticBonus.Where(e => e.DateEnd < DateTime.Now))
                    {
                        context.StaticBonus.Remove(entity);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
            }
        }

        public IEnumerable<StaticBonusDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (StaticBonus entity in context.StaticBonus.Where(i => i.CharacterId == characterId))
                {
                    yield return _mapper.Map<StaticBonusDTO>(entity);
                }
            }
        }

        #endregion

    }
}