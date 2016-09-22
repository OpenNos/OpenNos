using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class ShopSkillDAO : IShopSkillDAO
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