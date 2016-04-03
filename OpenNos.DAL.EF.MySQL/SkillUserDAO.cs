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
using OpenNos.Data.Enums;

namespace OpenNos.DAL.EF.MySQL
{
    public class SkillUserDAO : ISkillUserDAO
    {
        public DeleteResult Delete(long characterId, short skillVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                SkillUser invitem = context.skilluser.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVNum);
                if (invitem != null)
                {
                    context.skilluser.Remove(invitem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
        #region Methods

        public SkillUserDTO Insert(ref SkillUserDTO skilluser)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                SkillUser entity = Mapper.Map<SkillUser>(skilluser);
                context.skilluser.Add(entity);
                context.SaveChanges();
                return Mapper.Map<SkillUserDTO>(entity);
            }
        }

        public IEnumerable<SkillUserDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (SkillUser inventoryobject in context.skilluser.Where(i => i.CharacterId==characterId))
                {
                    yield return Mapper.Map<SkillUserDTO>(inventoryobject);
                }
            }
        }

        public IEnumerable<SkillUserDTO> LoadByMonsterNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (SkillUser skilluser in context.skilluser.Where(s => s.NpcMonsterVNum.Equals(npcId)))
                {
                    yield return Mapper.Map<SkillUserDTO>(skilluser);
                }
            }
        }

        #endregion
    }
}