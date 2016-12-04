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
    public class DropDao : MappingBaseDao<Drop, DropDTO>, IDropDAO
    {
        #region Methods

        public void Insert(List<DropDTO> drops)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (DropDTO drop in drops)
                    {
                        Drop entity = Mapper.Map<Drop>(drop);
                        context.Drop.Add(entity);
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

        public DropDTO Insert(DropDTO drop)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Drop entity = Mapper.Map<Drop>(drop);
                    context.Drop.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<DropDTO>(drop);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public List<DropDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return context.Drop.ToList().Select(d => Mapper.Map<DropDTO>(d)).ToList();
            }
        }

        public IEnumerable<DropDTO> LoadByMonster(short monsterVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Drop drop in context.Drop.Where(s => s.MonsterVNum == monsterVNum || s.MonsterVNum == null))
                {
                    yield return Mapper.Map<DropDTO>(drop);
                }
            }
        }

        #endregion
    }
}