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
    public class PortalDAO : IPortalDAO
    {
        #region Methods

        public void Insert(List<PortalDTO> portals)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (PortalDTO Item in portals)
                {
                    Portal entity = Mapper.DynamicMap<Portal>(Item);
                    context.Portal.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public PortalDTO Insert(PortalDTO portal)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Portal entity = Mapper.DynamicMap<Portal>(portal);
                context.Portal.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<PortalDTO>(entity);
            }
        }

        public IEnumerable<PortalDTO> LoadByMap(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Portal Portalobject in context.Portal.Where(c => c.SourceMapId.Equals(mapId)))
                {
                    yield return Mapper.DynamicMap<PortalDTO>(Portalobject);
                }
            }
        }

        #endregion
    }
}