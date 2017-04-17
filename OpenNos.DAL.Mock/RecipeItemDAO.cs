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

using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class RecipeItemDAO : BaseDAO<RecipeItemDTO>, IRecipeItemDAO
    {
        #region Methods

        public new RecipeItemDTO Insert(RecipeItemDTO recipeitem)
        {
            throw new NotImplementedException();
        }

        public new IEnumerable<RecipeItemDTO> LoadAll()
        {
            throw new NotImplementedException();
        }

        public RecipeItemDTO LoadById(short RecipeItemId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RecipeItemDTO> LoadByRecipe(short recipeId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RecipeItemDTO> LoadByRecipeAndItem(short recipeId, short itemVNum)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}