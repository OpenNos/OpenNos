using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

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