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
    public class RecipeDAO : IRecipeDAO
    {
        #region Methods

        public RecipeDTO Insert(RecipeDTO recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Recipe entity = Mapper.Map<Recipe>(recipe);
                context.recipe.Add(entity);
                context.SaveChanges();
                return Mapper.Map<RecipeDTO>(entity);
            }
        }

        public RecipeDTO LoadById(short RecipeId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<RecipeDTO>(context.recipe.FirstOrDefault(s => s.RecipeId.Equals(RecipeId)));
            }
        }

        public IEnumerable<RecipeDTO> LoadByNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Recipe recipe in context.recipe.Where(s => s.MapNpcId.Equals(npcId)))
                {
                    yield return Mapper.Map<RecipeDTO>(recipe);
                }
            }
        }

        public void Update(RecipeDTO recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Recipe result = context.recipe.SingleOrDefault(c => c.MapNpcId == recipe.MapNpcId && c.ItemVNum == recipe.ItemVNum);
                if (result != null)
                {
                    recipe.RecipeId = result.RecipeId;
                    result = Mapper.Map<RecipeDTO, Recipe>(recipe, result);
                    context.SaveChanges();
                }
            }
        }

        #endregion
    }
}