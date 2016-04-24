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

using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class MapMonsterDAO : IMapMonsterDAO
    {
        #region Methods

        public void Insert(List<MapMonsterDTO> monsters)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (MapMonsterDTO monster in monsters)
                {
                    MapMonster entity = Mapper.DynamicMap<MapMonster>(monster);
                    context.MapMonster.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public MapMonsterDTO Insert(MapMonsterDTO mapMonster)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                MapMonster entity = Mapper.DynamicMap<MapMonster>(mapMonster);
                context.MapMonster.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<MapMonsterDTO>(entity);
            }
        }

        public MapMonsterDTO LoadById(int monsterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<MapMonsterDTO>(context.MapMonster.FirstOrDefault(i => i.MapMonsterId.Equals(monsterId)));
            }
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MapMonster MapMonsterobject in context.MapMonster.Where(c => c.MapId.Equals(mapId)))
                {
                    yield return Mapper.DynamicMap<MapMonsterDTO>(MapMonsterobject);
                }
            }
        }

        #endregion
    }
}