using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class MapMonsterDAO : BaseDAO<MapMonsterDTO>, IMapMonsterDAO
    {
        #region Methods

        public MapMonsterDTO LoadById(int monsterId)
        {
            return Container.SingleOrDefault(m => m.MapMonsterId == monsterId);
        }

        public IEnumerable<MapMonsterDTO> LoadFromMap(short mapId)
        {
            return Container.Where(m => m.MapId == mapId);
        }

        #endregion
    }
}