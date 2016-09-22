using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class QuicklistEntryDAO : IQuicklistEntryDAO
    {
        #region Methods

        public DeleteResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QuicklistEntryDTO> InsertOrUpdate(IEnumerable<QuicklistEntryDTO> dtos)
        {
            throw new NotImplementedException();
        }

        public QuicklistEntryDTO InsertOrUpdate(QuicklistEntryDTO dto)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QuicklistEntryDTO> LoadByCharacterId(long characterId)
        {
            throw new NotImplementedException();
        }

        public QuicklistEntryDTO LoadById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}