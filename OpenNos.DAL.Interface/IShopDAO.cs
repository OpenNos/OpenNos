using OpenNos.Data;

namespace OpenNos.DAL.Interface
{
    public interface IShopDAO
    {
        #region Methods

        ShopDTO LoadById(int ShopId);

        ShopDTO LoadByNpc(short NpcId);

        ShopDTO Insert(ShopDTO shop);

        #endregion
    }
}