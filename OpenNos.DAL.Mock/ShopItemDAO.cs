using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class ShopItemDAO : BaseDAO<ShopItemDTO>, IShopItemDAO
    {
        #region Methods

        public DeleteResult DeleteById(int ItemId)
        {
            throw new NotImplementedException();
        }

        public void Insert(List<ShopItemDTO> items)
        {
            throw new NotImplementedException();
        }

        public ShopItemDTO Insert(ShopItemDTO item)
        {
            throw new NotImplementedException();
        }

        public ShopItemDTO LoadById(int ItemId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShopItemDTO> LoadByShopId(int ShopId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}