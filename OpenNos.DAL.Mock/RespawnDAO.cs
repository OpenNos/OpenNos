using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class RespawnDAO : IRespawnDAO
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