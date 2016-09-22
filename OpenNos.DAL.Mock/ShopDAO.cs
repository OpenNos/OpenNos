using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class ShopDAO : IShopDAO
    {
        #region Methods

        public void Insert(List<ShopDTO> shops)
        {
            throw new NotImplementedException();
        }

        public ShopDTO Insert(ShopDTO shop)
        {
            throw new NotImplementedException();
        }

        public ShopDTO LoadById(int ShopId)
        {
            throw new NotImplementedException();
        }

        public ShopDTO LoadByNpc(int NpcId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}