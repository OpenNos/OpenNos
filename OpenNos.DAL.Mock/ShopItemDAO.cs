using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class ShopItemDAO : IShopItemDAO
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