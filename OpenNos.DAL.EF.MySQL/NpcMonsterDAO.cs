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
    public class NpcMonsterDAO : INpcMonsterDAO
    {
        public void Insert(List<NpcMonsterDTO> npc)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (NpcMonsterDTO item in npc)
                {
                    NpcMonster entity = Mapper.Map<NpcMonster>(item);
                    context.npcmonster.Add(entity);
                }
                context.SaveChanges();

            }
        }
        #region Methods

        public NpcMonsterDTO Insert(NpcMonsterDTO npc)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                NpcMonster entity = Mapper.Map<NpcMonster>(npc);
                context.npcmonster.Add(entity);
                context.SaveChanges();
                return Mapper.Map<NpcMonsterDTO>(entity);
            }
        }

        public IEnumerable<NpcMonsterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonster npcmonster in context.npcmonster)
                {
                    yield return Mapper.Map<NpcMonsterDTO>(npcmonster);
                }
            }
        }

        public NpcMonsterDTO LoadById(short Vnum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<NpcMonsterDTO>(context.npcmonster.SingleOrDefault(i => i.NpcMonsterVNum.Equals(Vnum)));
            }
        }

        #endregion
    }
}