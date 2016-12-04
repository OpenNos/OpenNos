using System.Linq;
using OpenNos.Data;
using OpenNos.DAL.Interface;

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