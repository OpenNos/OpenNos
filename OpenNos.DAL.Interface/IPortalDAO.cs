using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface IPortalDAO
    {
        IEnumerable<PortalDTO> LoadFromMap(short MapId);
    }
}