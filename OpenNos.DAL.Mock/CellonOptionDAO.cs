using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class CellonOptionDAO : ICellonOptionDAO
    {
        #region Methods

        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(long inventoryitemId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}