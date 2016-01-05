using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IShopItemDAO
    {
        ShopItemDTO LoadById(int ItemId);
        DeleteResult DeleteById(int ItemId);
        SaveResult InsertOrUpdate(ref ShopItemDTO item);
    }
}