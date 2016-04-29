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
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public RecipeDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Recipe, RecipeDTO>();
                cfg.CreateMap<RecipeDTO, Recipe>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public RecipeDTO Insert(RecipeDTO recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Recipe entity = _mapper.Map<Recipe>(recipe);
                context.Recipe.Add(entity);
                context.SaveChanges();
                return _mapper.Map<RecipeDTO>(entity);
            }
        }

        public RecipeDTO LoadById(short recipeId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return _mapper.Map<RecipeDTO>(context.Recipe.FirstOrDefault(s => s.RecipeId.Equals(recipeId)));
            }
        }

        public IEnumerable<RecipeDTO> LoadByNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Recipe Recipe in context.Recipe.Where(s => s.MapNpcId.Equals(npcId)))
                {
                    yield return _mapper.Map<RecipeDTO>(Recipe);
                }
            }
        }

        public void Update(RecipeDTO recipe)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Recipe result = context.Recipe.FirstOrDefault(c => c.MapNpcId == recipe.MapNpcId && c.ItemVNum == recipe.ItemVNum);
                if (result != null)
                {
                    recipe.RecipeId = result.RecipeId;
                    _mapper.Map(recipe, result);
                    context.SaveChanges();
                }
            }
        }

        #endregion
    }
}