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

namespace OpenNos.DAL.EF
{
    public class StaticBonusDAO : MappingBaseDAO<StaticBonus, StaticBonusDTO>, IStaticBonusDAO
    {
        #region Methods

        public DeleteResult Delete(long staticBonusId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    StaticBonus entity = context.StaticBonus.FirstOrDefault(i => i.StaticBonusId == staticBonusId);
                    if (entity != null)
                    {
                        context.StaticBonus.Remove(entity);
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

        public StaticBonusDTO Insert(StaticBonusDTO staticBonus)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    StaticBonus entity = _mapper.Map<StaticBonus>(staticBonus);
                    context.StaticBonus.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<StaticBonusDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public StaticBonusDTO LoadByCharacterId(long characterId)
        {
            {
                try
                {
                    using (var context = DataAccessHelper.CreateContext())
                    {
                        return _mapper.Map<StaticBonusDTO>(context.StaticBonus.FirstOrDefault(i => i.CharacterId.Equals(characterId)));
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return null;
                }
            }
        }

        #endregion
    }
}