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
    public class MapDAO : IMapDAO
    {
        #region Methods

        public void Insert(List<MapDTO> Maps)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MapDTO Item in Maps)
                {
                    Map entity = Mapper.Map<Map>(Item);
                    context.Map.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public MapDTO Insert(MapDTO Map)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                if (context.Map.FirstOrDefault(c => c.MapId.Equals(Map.MapId)) == null)
                {
                    Map entity = Mapper.Map<Map>(Map);
                    context.Map.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<MapDTO>(entity);
                }
                else return new MapDTO();
            }
        }

        public IEnumerable<MapDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Map Map in context.Map)
                {
                    yield return Mapper.Map<MapDTO>(Map);
                }
            }
        }

        public MapDTO LoadById(short MapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<MapDTO>(context.Map.FirstOrDefault(c => c.MapId.Equals(MapId)));
            }
        }

        #endregion
    }
}