using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class RecipeDAO : BaseDAO<RecipeDTO>, IRecipeDAO
    {
        #region Methods

        public RecipeDTO Insert(RecipeDTO recipe)
        {
            throw new NotImplementedException();
        }

        public RecipeDTO LoadById(short RecipeId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RecipeDTO> LoadByNpc(int npcId)
        {
            throw new NotImplementedException();
        }

        public void Update(RecipeDTO recipe)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}