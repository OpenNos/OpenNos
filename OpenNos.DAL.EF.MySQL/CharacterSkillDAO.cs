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
    public class CharacterSkillDAO : ICharacterSkillDAO
    {
        public DeleteResult Delete(long characterId, short skillVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                CharacterSkill invitem = context.characterskill.FirstOrDefault(i => i.CharacterId == characterId && i.SkillVNum == skillVNum);
                if (invitem != null)
                {
                    context.characterskill.Remove(invitem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
        #region Methods

        public CharacterSkillDTO Insert(ref CharacterSkillDTO characterskill)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                CharacterSkill entity = Mapper.Map<CharacterSkill>(characterskill);
                context.characterskill.Add(entity);
                context.SaveChanges();
                return Mapper.Map<CharacterSkillDTO>(entity);
            }
        }

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterSkill inventoryobject in context.characterskill.Where(i => i.CharacterId==characterId))
                {
                    yield return Mapper.Map<CharacterSkillDTO>(inventoryobject);
                }
            }
        }


        #endregion
    }
}