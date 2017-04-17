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

using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class MapMonsterDAO : BaseDAO<MapMonsterDTO>, IMapMonsterDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapMonsterId)
        {
            throw new NotImplementedException();
        }

        public bool DoesMonsterExist(int mapMonsterId)
        {
            throw new NotImplementedException();
        }

        public new void Insert(IEnumerable<MapMonsterDTO> monsters)
        {
            throw new NotImplementedException();
        }

        public MapMonsterDTO LoadById(int mapMonsterId)
        {
            return Container.SingleOrDefault(m => m.MapMonsterId == mapMonsterId);
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short mapId)
        {
            return Container.Where(m => m.MapId == mapId);
        }

        #endregion
    }
}