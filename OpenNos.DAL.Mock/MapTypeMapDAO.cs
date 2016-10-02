using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class MapTypeMapDAO : BaseDAO<MapTypeMapDTO>, IMapTypeMapDAO
    {
        #region Methods

        public MapTypeMapDTO LoadByMapAndMapType(short mapId, short maptypeId)
        {
            return Container.SingleOrDefault(m => m.MapId == mapId && m.MapTypeId == maptypeId);
        }

        public IEnumerable<MapTypeMapDTO> LoadByMapId(short mapId)
        {
            return Container.Where(m => m.MapId == mapId);
        }

        public IEnumerable<MapTypeMapDTO> LoadByMapTypeId(short maptypeId)
        {
            return Container.Where(m => m.MapTypeId == maptypeId);
        }

        #endregion
    }
}