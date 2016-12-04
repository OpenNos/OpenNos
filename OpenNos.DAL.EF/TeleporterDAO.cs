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
    public class TeleporterDao : MappingBaseDao<Teleporter, TeleporterDTO>, ITeleporterDAO
    {
        #region Methods

        public TeleporterDTO Insert(TeleporterDTO teleporter)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Teleporter entity = Mapper.Map<Teleporter>(teleporter);
                    context.Teleporter.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<TeleporterDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<TeleporterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Teleporter entity in context.Teleporter)
                {
                    yield return Mapper.Map<TeleporterDTO>(entity);
                }
            }
        }

        public TeleporterDTO LoadById(short teleporterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return Mapper.Map<TeleporterDTO>(context.Teleporter.FirstOrDefault(i => i.TeleporterId.Equals(teleporterId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<TeleporterDTO> LoadFromNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Teleporter entity in context.Teleporter.Where(c => c.MapNpcId.Equals(npcId)))
                {
                    yield return Mapper.Map<TeleporterDTO>(entity);
                }
            }
        }

        #endregion
    }
}