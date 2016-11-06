using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class QuicklistEntryDAO : SynchronizableBaseDAO<QuicklistEntryDTO>, IQuicklistEntryDAO
    {
        #region Methods

        public IEnumerable<QuicklistEntryDTO> LoadByCharacterId(long characterId)
        {
            return Container.Where(c => c.CharacterId == characterId);
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            return Container.Where(c => c.CharacterId == characterId).Select(c => c.Id);
        }

        #endregion
    }
}