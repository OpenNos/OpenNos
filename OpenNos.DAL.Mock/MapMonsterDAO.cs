using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class MapMonsterDAO : BaseDAO<MapMonsterDTO>, IMapMonsterDAO
    {
        #region Methods

        public DeleteResult DeleteById(int mapMonsterId)
        {
            throw new NotImplementedException();
        }

        public MapMonsterDTO LoadById(int mapMonsterId)
        {
            return Container.SingleOrDefault(m => m.MapMonsterId == mapMonsterId);
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short mapId)
        {
            return Container.Where(m => m.MapId == mapId);
        }

        #endregion
    }
}