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
    public class MapDAO : IMapDAO
    {
        #region Methods

        public IEnumerable<MapDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Map map in context.map)
                {
                    yield return Mapper.Map<MapDTO>(map);
                }
            }
        }

        public MapDTO LoadById(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<MapDTO>(context.map.SingleOrDefault(c => c.MapId.Equals(mapId)));
            }
        }

        public MapDTO Insert(MapDTO map)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Map entity = Mapper.Map<Map>(map);
                context.map.Add(entity);
                context.SaveChanges();
                return Mapper.Map<MapDTO>(entity);
            }
        }
        #endregion
    }
}