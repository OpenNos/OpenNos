using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class MapTypeMapDAO : IMapTypeMapDAO
    {
        #region Methods

        public void Insert(List<MapTypeMapDTO> maptypemap)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapTypeMapDTO> LoadAll()
        {
            throw new NotImplementedException();
        }

        public MapTypeMapDTO LoadByMapAndMapType(short mapId, short maptypeId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapTypeMapDTO> LoadByMapId(short mapId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapTypeMapDTO> LoadByMapTypeId(short maptypeId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}