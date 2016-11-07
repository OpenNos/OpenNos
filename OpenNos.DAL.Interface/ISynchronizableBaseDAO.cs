using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface ISynchronizableBaseDAO<TDTO> : IMappingBaseDAO
        where TDTO : SynchronizableBaseDTO
    {
        #region Methods

        DeleteResult Delete(Guid id);

        TDTO InsertOrUpdate(TDTO dto);

        IEnumerable<TDTO> InsertOrUpdate(IEnumerable<TDTO> dtos);

        TDTO LoadById(Guid id);

        #endregion
    }
}