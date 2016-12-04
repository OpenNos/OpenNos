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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class PortalDao : MappingBaseDao<Portal, PortalDTO>, IPortalDAO
    {
        #region Methods

        public void Insert(List<PortalDTO> portals)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (PortalDTO item in portals)
                    {
                        Portal entity = Mapper.Map<Portal>(item);
                        context.Portal.Add(entity);
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

        public PortalDTO Insert(PortalDTO portal)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Portal entity = Mapper.Map<Portal>(portal);
                    context.Portal.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<PortalDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<PortalDTO> LoadByMap(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Portal portalobject in context.Portal.Where(c => c.SourceMapId.Equals(mapId)))
                {
                    yield return Mapper.Map<PortalDTO>(portalobject);
                }
            }
        }

        #endregion
    }
}