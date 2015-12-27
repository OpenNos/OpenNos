using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IInventoryDAO
    {
        DeleteResult DeleteFromSlotAndType(long characterId, short slot, short type);
        SaveResult InsertOrUpdate(ref InventoryDTO inventory);
        InventoryDTO LoadBySlotAndType(long characterId, short slot, short type);
        IEnumerable<InventoryDTO> LoadByType(long characterId, short type);
        IEnumerable<InventoryDTO> LoadByCharacterId(long characterId);
        short getFirstPlace(long characterId, byte type,int backpack);
    }
}