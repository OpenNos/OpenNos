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
    public class RecipeDAO : IRecipeDAO
    {
        #region Methods

        public RecipeDTO Insert(RecipeDTO Recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Recipe entity = Mapper.DynamicMap<Recipe>(Recipe);
                context.Recipe.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<RecipeDTO>(entity);
            }
        }

        public RecipeDTO LoadById(short RecipeId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<RecipeDTO>(context.Recipe.FirstOrDefault(s => s.RecipeId.Equals(RecipeId)));
            }
        }

        public IEnumerable<RecipeDTO> LoadByNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Recipe Recipe in context.Recipe.Where(s => s.MapNpcId.Equals(npcId)))
                {
                    yield return Mapper.DynamicMap<RecipeDTO>(Recipe);
                }
            }
        }

        public void Update(RecipeDTO Recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Recipe result = context.Recipe.FirstOrDefault(c => c.MapNpcId == Recipe.MapNpcId && c.ItemVNum == Recipe.ItemVNum);
                if (result != null)
                {
                    Recipe.RecipeId = result.RecipeId;
                    result = Mapper.Map<RecipeDTO, Recipe>(Recipe, result);
                    context.SaveChanges();
                }
            }
        }

        #endregion
    }
}