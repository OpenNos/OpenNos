using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IItemInstanceDAO
    {
        ItemInstanceDTO LoadById(long ItemId);
    }
}