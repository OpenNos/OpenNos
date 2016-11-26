using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class RespawnMapTypeDAO : BaseDAO<RespawnMapTypeDAO>, IRespawnMapTypeDAO
    {
        public void Insert(List<RespawnMapTypeDTO> respawnmaptypemaps)
        {
            throw new NotImplementedException();
        }
        #region Methods

        public SaveResult InsertOrUpdate(ref RespawnMapTypeDTO respawnMapType)
        {
            throw new NotImplementedException();
        }

        public RespawnMapTypeDTO LoadById(long respawnMapTypeId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RespawnMapTypeDTO> LoadByMapId(short mapId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}