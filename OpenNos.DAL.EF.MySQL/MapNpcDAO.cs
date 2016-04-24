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
    public class MapNpcDAO : IMapNpcDAO
    {
        #region Methods

        public void Insert(List<MapNpcDTO> npcs)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (MapNpcDTO Item in npcs)
                {
                    MapNpc entity = Mapper.DynamicMap<MapNpc>(Item);
                    context.MapNpc.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public MapNpcDTO Insert(MapNpcDTO npc)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                MapNpc entity = Mapper.DynamicMap<MapNpc>(npc);
                context.MapNpc.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<MapNpcDTO>(entity);
            }
        }

        public MapNpcDTO LoadById(int id)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<MapNpcDTO>(context.MapNpc.FirstOrDefault(i => i.MapNpcId.Equals(id)));
            }
        }

        public IEnumerable<MapNpcDTO> LoadFromMap(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MapNpc npcobject in context.MapNpc.Where(c => c.MapId.Equals(mapId)))
                {
                    yield return Mapper.DynamicMap<MapNpcDTO>(npcobject);
                }
            }
        }

        #endregion
    }
}