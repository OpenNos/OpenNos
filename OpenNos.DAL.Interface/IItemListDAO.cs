using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IItemListDAO
    {
        ItemListDTO LoadById(short Vnum);
    }
}