using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IRespawnDAO
    {
        #region Methods

        RespawnDTO LoadById(long respawnId);

        IEnumerable<RespawnDTO> LoadByCharacterId(long characterId);

        SaveResult InsertOrUpdate(ref RespawnDTO respawn);

        #endregion
    }
}
