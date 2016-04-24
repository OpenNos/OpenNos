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
    public class NpcMonsterDAO : INpcMonsterDAO
    {
        #region Methods

        public void Insert(List<NpcMonsterDTO> npcs)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (NpcMonsterDTO Item in npcs)
                {
                    NpcMonster entity = Mapper.DynamicMap<NpcMonster>(Item);
                    context.NpcMonster.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public NpcMonsterDTO Insert(NpcMonsterDTO npc)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                NpcMonster entity = Mapper.DynamicMap<NpcMonster>(npc);
                context.NpcMonster.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<NpcMonsterDTO>(entity);
            }
        }

        public IEnumerable<NpcMonsterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonster NpcMonster in context.NpcMonster)
                {
                    yield return Mapper.DynamicMap<NpcMonsterDTO>(NpcMonster);
                }
            }
        }

        public NpcMonsterDTO LoadByVnum(short vnum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<NpcMonsterDTO>(context.NpcMonster.FirstOrDefault(i => i.NpcMonsterVNum.Equals(vnum)));
            }
        }

        #endregion
    }
}