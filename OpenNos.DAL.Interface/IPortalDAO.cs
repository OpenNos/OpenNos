using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IPortalDAO
    {
        #region Methods

        IEnumerable<PortalDTO> LoadFromMap(short MapId);

        PortalDTO Insert(PortalDTO portal);
        #endregion
    }
}