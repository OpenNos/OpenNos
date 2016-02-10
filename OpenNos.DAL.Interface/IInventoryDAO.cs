using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IInventoryDAO
    {
        #region Methods

        DeleteResult DeleteFromSlotAndType(long characterId, short slot, short type);

        SaveResult InsertOrUpdate(ref InventoryDTO inventory);

        IEnumerable<InventoryDTO> LoadByCharacterId(long characterId);

        InventoryDTO LoadByInventoryItem(long InventoryItemId);

        InventoryDTO LoadBySlotAndType(long characterId, short slot, short type);

        IEnumerable<InventoryDTO> LoadByType(long characterId, short type);

        #endregion
    }
}