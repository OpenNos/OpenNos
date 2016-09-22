using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class MapDAO : BaseDAO<MapDTO>, IMapDAO
    {
        #region Methods

        public MapDTO LoadById(short mapId)
        {
            return Container.SingleOrDefault(c => c.MapId.Equals(mapId));
        }

        #endregion
    }
}