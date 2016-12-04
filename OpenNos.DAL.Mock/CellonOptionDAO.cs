using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class CellonOptionDAO : SynchronizableBaseDAO<CellonOptionDTO>, ICellonOptionDAO
    {
        #region Methods

        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(long inventoryitemId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}