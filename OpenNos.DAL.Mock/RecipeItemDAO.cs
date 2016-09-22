using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class RecipeItemDAO : IRecipeItemDAO
    {
        #region Methods

        public RecipeItemDTO Insert(RecipeItemDTO recipeitem)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RecipeItemDTO> LoadAll()
        {
            throw new NotImplementedException();
        }

        public RecipeItemDTO LoadById(int RecipeItemId)
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