using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class PortalDAO : BaseDAO<PortalDTO>, IPortalDAO
    {
        #region Methods

        public IEnumerable<PortalDTO> LoadByMap(short mapId)
        {
            return Container.Where(p => p.SourceMapId == mapId);
        }

        #endregion
    }
}