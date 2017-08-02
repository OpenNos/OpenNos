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
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class StaticBonusDAO : MappingBaseDAO<StaticBonus, StaticBonusDTO>, IStaticBonusDAO
    {

        #region Methods

        public void Delete(short bonusToDelete, long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    StaticBonus bon = context.StaticBonus.FirstOrDefault(c => c.StaticBonusType == (StaticBonusType)bonusToDelete && c.CharacterId == characterId);

                    if (bon != null)
                    {
                        context.StaticBonus.Remove(bon);
                        context.SaveChanges();
                    }
                    
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bonusToDelete, e.Message), e);
            }
        }

        public SaveResult InsertOrUpdate(ref StaticBonusDTO staticBonus)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long id = staticBonus.CharacterId;
                    StaticBonusType cardid = staticBonus.StaticBonusType;
                    StaticBonus entity = context.StaticBonus.FirstOrDefault(c => c.StaticBonusType == cardid && c.CharacterId == id);

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

        public IEnumerable<StaticBonusDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (StaticBonus entity in context.StaticBonus.Where(i => i.CharacterId == characterId && i.DateEnd > DateTime.Now))
                {
                    yield return _mapper.Map<StaticBonusDTO>(entity);
                }
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

        public IEnumerable<short> LoadTypeByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.StaticBonus.Where(i => i.CharacterId == characterId).Select(qle => (short)qle.StaticBonusType).ToList();
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

        #endregion
    }
}