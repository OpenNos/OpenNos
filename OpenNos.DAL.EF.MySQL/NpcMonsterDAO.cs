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
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class NpcMonsterDAO : INpcMonsterDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public NpcMonsterDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NpcMonster, NpcMonsterDTO>();
                cfg.CreateMap<NpcMonsterDTO, NpcMonster>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public IEnumerable<NpcMonsterDTO> FindByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonster NpcMonster in context.NpcMonster.Where(s => s.Name.Contains(name)))
                {
                    yield return _mapper.Map<NpcMonsterDTO>(NpcMonster);
                }
            }
        }

        public void Insert(List<NpcMonsterDTO> npcs)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (NpcMonsterDTO Item in npcs)
                    {
                        NpcMonster entity = _mapper.Map<NpcMonster>(Item);
                        context.NpcMonster.Add(entity);
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

        public NpcMonsterDTO Insert(NpcMonsterDTO npc)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    NpcMonster entity = _mapper.Map<NpcMonster>(npc);
                    context.NpcMonster.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<NpcMonsterDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<NpcMonsterDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (NpcMonster NpcMonster in context.NpcMonster)
                {
                    yield return _mapper.Map<NpcMonsterDTO>(NpcMonster);
                }
            }
        }

        public NpcMonsterDTO LoadByVnum(short vnum)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<NpcMonsterDTO>(context.NpcMonster.FirstOrDefault(i => i.NpcMonsterVNum.Equals(vnum)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}