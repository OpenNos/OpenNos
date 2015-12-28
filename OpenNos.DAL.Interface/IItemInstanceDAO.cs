using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IItemInstanceDAO
    {
        ItemInstanceDTO LoadById(short ItemId);
        DeleteResult DeleteById(short ItemId);
        SaveResult InsertOrUpdate(ref ItemInstanceDTO item);
        IEnumerable<ItemInstanceDTO> LoadBySlotAllowed(long characterId, short itemVNum, short amount);
    }
}