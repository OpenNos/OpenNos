using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IInventoryDAO
    {
        InventoryDTO LoadBySlotAndType(long characterId, short slot, short type);
        IEnumerable<InventoryDTO> LoadBySlot(long characterId, short slot);
        IEnumerable<InventoryDTO> LoadByType(long characterId, short type);
        IEnumerable<InventoryDTO> Load(long characterId);
    }
}