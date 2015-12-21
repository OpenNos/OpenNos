using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IItemDAO
    {
        ItemDTO LoadById(short ItemId);
    }
}