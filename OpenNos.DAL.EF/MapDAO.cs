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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class MapDAO : MappingBaseDAO<Map, MapDTO>, IMapDAO
    {
        #region Methods

        public void Insert(List<MapDTO> maps)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (MapDTO Item in maps)
                    {
                        Map entity = _mapper.Map<Map>(Item);
                        context.Map.Add(entity);
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

        public MapDTO Insert(MapDTO map)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    if (context.Map.FirstOrDefault(c => c.MapId.Equals(map.MapId)) == null)
                    {
                        Map entity = _mapper.Map<Map>(map);
                        context.Map.Add(entity);
                        context.SaveChanges();
                        return _mapper.Map<MapDTO>(entity);
                    }
                    return new MapDTO();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<MapDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Map Map in context.Map)
                {
                    yield return _mapper.Map<MapDTO>(Map);
                }
            }
        }

        public MapDTO LoadById(short mapId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<MapDTO>(context.Map.FirstOrDefault(c => c.MapId.Equals(mapId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}