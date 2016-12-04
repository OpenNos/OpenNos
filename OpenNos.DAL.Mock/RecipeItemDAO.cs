using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class RecipeItemDAO : BaseDAO<RecipeItemDTO>, IRecipeItemDAO
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