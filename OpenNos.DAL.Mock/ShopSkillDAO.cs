using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class ShopSkillDAO : BaseDAO<ShopSkillDTO>, IShopSkillDAO
    {
        #region Methods

        public void Insert(List<ShopSkillDTO> skills)
        {
            throw new NotImplementedException();
        }

        public ShopSkillDTO Insert(ShopSkillDTO shopskill)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShopSkillDTO> LoadByShopId(int ShopId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}