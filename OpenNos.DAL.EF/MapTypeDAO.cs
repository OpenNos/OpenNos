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
    public class MapTypeDAO : MappingBaseDAO<MapType, MapTypeDTO>, IMapTypeDAO
    {
        #region Methods

        public MapTypeDTO Insert(ref MapTypeDTO mapType)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    MapType entity = _mapper.Map<MapType>(mapType);
                    context.MapType.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<MapTypeDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<MapTypeDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MapType MapType in context.MapType)
                {
                    yield return _mapper.Map<MapTypeDTO>(MapType);
                }
            }
        }

        public MapTypeDTO LoadById(short maptypeId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<MapTypeDTO>(context.MapType.FirstOrDefault(s => s.MapTypeId.Equals(maptypeId)));
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