using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class MapTypeDAO : IMapTypeDAO
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