using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class ShopDAO : BaseDAO<ShopDTO>, IShopDAO
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