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
                    MapMonster entity = Mapper.Map<MapMonster>(monster);
                    context.mapmonster.Add(entity);
                }
                context.SaveChanges();

            }
        }

        public MapMonsterDTO Insert(MapMonsterDTO mapmonster)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                MapMonster entity = Mapper.Map<MapMonster>(mapmonster);
                context.mapmonster.Add(entity);
                context.SaveChanges();
                return Mapper.Map<MapMonsterDTO>(entity);
            }
        }

        public MapMonsterDTO LoadById(int MonsterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<MapMonsterDTO>(context.mapmonster.SingleOrDefault(i => i.MapMonsterId.Equals(MonsterId)));
            }
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short MapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MapMonster mapmonsterobject in context.mapmonster.Where(c => c.MapId.Equals(MapId)))
                {
                    yield return Mapper.Map<MapMonsterDTO>(mapmonsterobject);
                }
            }
        }

        #endregion
    }
}