using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

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