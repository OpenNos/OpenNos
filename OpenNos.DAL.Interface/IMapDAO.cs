using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IMapDAO
    {
        MapDTO LoadById(short mapId);
        IEnumerable<MapDTO> LoadAll();
    }
}