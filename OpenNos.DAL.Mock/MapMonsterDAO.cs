using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class MapMonsterDAO : IMapMonsterDAO
    {
        #region Methods

        public void Insert(List<MapMonsterDTO> monsters)
        {
            throw new NotImplementedException();
        }

        public MapMonsterDTO Insert(MapMonsterDTO mapmonster)
        {
            throw new NotImplementedException();
        }

        public MapMonsterDTO LoadById(int MonsterId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short MapId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}