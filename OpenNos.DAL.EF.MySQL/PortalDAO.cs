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
using System;

namespace OpenNos.DAL.EF.MySQL
{
    public class PortalDAO : IPortalDAO
    {
        public void Insert(List<PortalDTO> portals)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (PortalDTO item in portals)
                {
                    Portal entity = Mapper.Map<Portal>(item);
                    context.portal.Add(entity);
                }
                context.SaveChanges();

            }
        }
        #region Methods

        public PortalDTO Insert(PortalDTO portal)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Portal entity = Mapper.Map<Portal>(portal);
                context.portal.Add(entity);
                context.SaveChanges();
                return Mapper.Map<PortalDTO>(entity);
            }
        }

        public IEnumerable<PortalDTO> LoadByMap(short MapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Portal portalobject in context.portal.Where(c => c.SourceMapId.Equals(MapId)))
                {
                    yield return Mapper.Map<PortalDTO>(portalobject);
                }
            }
        }

        #endregion
    }
}