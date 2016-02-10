using OpenNos.Data;

namespace OpenNos.DAL.Interface
{
    public interface IInventoryItemDAO
    {
        #region Methods

        DeleteResult DeleteById(long ItemId);

        SaveResult InsertOrUpdate(ref InventoryItemDTO item);

        InventoryItemDTO LoadById(long ItemId);

        #endregion
    }
}