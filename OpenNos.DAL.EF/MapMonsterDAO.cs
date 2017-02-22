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
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class MapMonsterDAO : MappingBaseDAO<MapMonster, MapMonsterDTO>, IMapMonsterDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapMonsterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    MapMonster monster = context.MapMonster.First(i => i.MapMonsterId.Equals(mapMonsterId));

                    if (monster != null)
                    {
                        context.MapMonster.Remove(monster);
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

        public bool DoesMonsterExist(int mapMonsterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return context.MapMonster.Any(i => i.MapMonsterId.Equals(mapMonsterId));
            }
        }

        public void Insert(IEnumerable<MapMonsterDTO> monsters)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (MapMonsterDTO monster in monsters)
                    {
                        MapMonster entity = _mapper.Map<MapMonster>(monster);
                        context.MapMonster.Add(entity);
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

        public MapMonsterDTO Insert(MapMonsterDTO mapMonster)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    MapMonster entity = _mapper.Map<MapMonster>(mapMonster);
                    context.MapMonster.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<MapMonsterDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public MapMonsterDTO LoadById(int mapMonsterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<MapMonsterDTO>(context.MapMonster.FirstOrDefault(i => i.MapMonsterId.Equals(mapMonsterId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MapMonster MapMonsterobject in context.MapMonster.Where(c => c.MapId.Equals(mapId)))
                {
                    yield return _mapper.Map<MapMonsterDTO>(MapMonsterobject);
                }
            }
        }

        #endregion
    }
}