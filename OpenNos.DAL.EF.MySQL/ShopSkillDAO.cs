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

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopSkillDAO : IShopSkillDAO
    {
        #region Methods

        public ShopSkillDTO Insert(ShopSkillDTO shopskill)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopSkill entity = Mapper.Map<ShopSkill>(shopskill);
                context.shopskill.Add(entity);
                context.SaveChanges();
                return Mapper.Map<ShopSkillDTO>(entity);
            }
        }
        public void Insert(List<ShopSkillDTO> skills)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ShopSkillDTO skill in skills)
                {
                    ShopSkill entity = Mapper.Map<ShopSkill>(skill);
                    context.shopskill.Add(entity);
                }
                context.SaveChanges();

            }
        }
        public IEnumerable<ShopSkillDTO> LoadByShopId(int ShopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopSkill shopskill in context.shopskill.Where(s => s.ShopId.Equals(ShopId)))
                {
                    yield return Mapper.Map<ShopSkillDTO>(shopskill);
                }
            }
        }

        #endregion
    }
}