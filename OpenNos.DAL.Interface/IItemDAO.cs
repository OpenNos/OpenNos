using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IItemDAO
    {
        #region Methods

        IEnumerable<ItemDTO> LoadAll();

        ItemDTO LoadById(short Vnum);

        #endregion
    }
}