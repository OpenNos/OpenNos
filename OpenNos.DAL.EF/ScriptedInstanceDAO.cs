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
    public class ScriptedInstanceDAO : MappingBaseDAO<ScriptedInstance, ScriptedInstanceDTO>, IScriptedInstanceDAO
    {
        #region Methods

        public void Insert(List<ScriptedInstanceDTO> portals)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ScriptedInstanceDTO Item in portals)
                    {
                        ScriptedInstance entity = _mapper.Map<ScriptedInstance>(Item);
                        context.ScriptedInstance.Add(entity);
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

        public ScriptedInstanceDTO Insert(ScriptedInstanceDTO timespace)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    ScriptedInstance entity = _mapper.Map<ScriptedInstance>(timespace);
                    context.ScriptedInstance.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<ScriptedInstanceDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ScriptedInstanceDTO> LoadByMap(short mapId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ScriptedInstance timespaceObject in context.ScriptedInstance.Where(c => c.MapId.Equals(mapId)))
                {
                    yield return _mapper.Map<ScriptedInstanceDTO>(timespaceObject);
                }
            }
        }

        #endregion
    }
}