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
    public class ComboDAO : IComboDAO
    {
        #region Methods

        public void Insert(List<ComboDTO> combos)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ComboDTO Combo in combos)
                {
                    Combo entity = Mapper.DynamicMap<Combo>(Combo);
                    context.Combo.Add(entity);
                }
                context.SaveChanges();

            }
        }

        public ComboDTO Insert(ComboDTO combo)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Combo entity = Mapper.DynamicMap<Combo>(combo);
                context.Combo.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<ComboDTO>(entity);
            }
        }

        public IEnumerable<ComboDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Combo Combo in context.Combo)
                {
                    yield return Mapper.DynamicMap<ComboDTO>(Combo);
                }
            }
        }
        public ComboDTO LoadById(short comboId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<ComboDTO>(context.Combo.FirstOrDefault(s => s.SkillVNum.Equals(comboId)));
            }
        }

        #endregion
    }
}
