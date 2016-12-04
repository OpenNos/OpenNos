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
    public class ShopSkillDao : MappingBaseDao<ShopSkill, ShopSkillDTO>, IShopSkillDAO
    {
        #region Methods

        public ShopSkillDTO Insert(ShopSkillDTO shopSkill)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    ShopSkill entity = Mapper.Map<ShopSkill>(shopSkill);
                    context.ShopSkill.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<ShopSkillDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void Insert(List<ShopSkillDTO> skills)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ShopSkillDTO skill in skills)
                    {
                        ShopSkill entity = Mapper.Map<ShopSkill>(skill);
                        context.ShopSkill.Add(entity);
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

        public IEnumerable<ShopSkillDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopSkill entity in context.ShopSkill)
                {
                    yield return Mapper.Map<ShopSkillDTO>(entity);
                }
            }
        }

        public IEnumerable<ShopSkillDTO> LoadByShopId(int shopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopSkill shopSkill in context.ShopSkill.Where(s => s.ShopId.Equals(shopId)))
                {
                    yield return Mapper.Map<ShopSkillDTO>(shopSkill);
                }
            }
        }

        #endregion
    }
}