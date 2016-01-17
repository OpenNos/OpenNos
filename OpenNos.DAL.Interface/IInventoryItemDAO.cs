using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IInventoryItemDAO
    {
        InventoryItemDTO LoadById(long ItemId);
        DeleteResult DeleteById(short ItemId);
        SaveResult InsertOrUpdate(ref InventoryItemDTO item);
    }
}