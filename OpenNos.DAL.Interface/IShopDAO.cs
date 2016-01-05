using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IShopDAO
    {
        ShopDTO LoadById(int ShopItemId);
        IEnumerable<ShopDTO> LoadByNpc(short NpcId);
    }
}