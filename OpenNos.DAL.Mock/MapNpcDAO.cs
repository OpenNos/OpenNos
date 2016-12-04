using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class MapNpcDAO : BaseDAO<MapNpcDTO>, IMapNpcDAO
    {
        #region Methods

        public MapNpcDTO LoadById(int mapNpcId)
        {
            return Container.SingleOrDefault(n => n.MapNpcId == mapNpcId);
        }

        public IEnumerable<MapNpcDTO> LoadFromMap(short mapId)
        {
            return Container.Where(n => n.MapId == mapId);
        }

        #endregion
    }
}