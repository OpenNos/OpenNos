using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class MapNpcDAO : IMapNpcDAO
    {
        #region Methods

        public void Insert(List<MapNpcDTO> npcs)
        {
            throw new NotImplementedException();
        }

        public MapNpcDTO Insert(MapNpcDTO npc)
        {
            throw new NotImplementedException();
        }

        public MapNpcDTO LoadById(int MapNpcId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapNpcDTO> LoadFromMap(short MapId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}