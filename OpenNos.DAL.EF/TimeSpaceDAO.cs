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
        public class TimeSpaceDAO : MappingBaseDAO<TimeSpace, TimeSpaceDTO>, ITimeSpaceDAO
        {
            #region Methods

            public void Insert(List<TimeSpaceDTO> portals)
            {
                try
                {
                    using (var context = DataAccessHelper.CreateContext())
                    {
                        context.Configuration.AutoDetectChangesEnabled = false;
                        foreach (TimeSpaceDTO Item in portals)
                        {
                            TimeSpace entity = _mapper.Map<TimeSpace>(Item);
                            context.TimeSpace.Add(entity);
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

            public TimeSpaceDTO Insert(TimeSpaceDTO timespace)
            {
                try
                {
                    using (var context = DataAccessHelper.CreateContext())
                    {
                        TimeSpace entity = _mapper.Map<TimeSpace>(timespace);
                        context.TimeSpace.Add(entity);
                        context.SaveChanges();
                        return _mapper.Map<TimeSpaceDTO>(entity);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return null;
                }
            }

            public IEnumerable<TimeSpaceDTO> LoadByMap(short mapId)
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (TimeSpace timespaceObject in context.TimeSpace.Where(c => c.MapId.Equals(mapId)))
                    {
                        yield return _mapper.Map<TimeSpaceDTO>(timespaceObject);
                    }
                }
            }

            #endregion
        }
    }
}
}
