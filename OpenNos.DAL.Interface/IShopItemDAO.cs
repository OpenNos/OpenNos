using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IShopItemDAO
    {
        #region Methods

        DeleteResult DeleteById(int ItemId);

        SaveResult InsertOrUpdate(ref ShopItemDTO item);

        ShopItemDTO LoadById(int ItemId);

        IEnumerable<ShopItemDTO> LoadByShopId(int ShopId);

        #endregion
    }
}