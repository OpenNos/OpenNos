using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class MapTypeDAO : BaseDAO<MapTypeDTO>, IMapTypeDAO
    {
        #region Methods

        public MapTypeDTO Insert(ref MapTypeDTO mapType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapTypeDTO> LoadAll()
        {
            throw new NotImplementedException();
        }

        public MapTypeDTO LoadById(short maptypeId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}