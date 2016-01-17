using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IShopDAO
    {
        ShopDTO LoadById(int ShopId);
        ShopDTO LoadByNpc(short NpcId);
    }
}