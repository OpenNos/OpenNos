using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class RespawnDAO : BaseDAO<RespawnDTO>, IRespawnDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnDTO respawn)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RespawnDTO> LoadByCharacter(long characterId)
        {
            throw new NotImplementedException();
        }

        public RespawnDTO LoadById(long respawnId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}