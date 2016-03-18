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
    public class RecipeDAO : IRecipeDAO
    {
        #region Methods

        public RecipeDTO Insert(RecipeDTO recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                if (context.shop.SingleOrDefault(c => c.MapNpcId.Equals(recipe.MapNpcId)) == null)
                {
                    Recipe entity = Mapper.Map<Recipe>(recipe);
                    context.recipe.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<RecipeDTO>(entity);
                }
                else return new RecipeDTO();
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

        #endregion
    }
}