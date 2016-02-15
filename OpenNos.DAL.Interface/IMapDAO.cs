using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IMapDAO
    {
        #region Methods

        IEnumerable<MapDTO> LoadAll();

        MapDTO LoadById(short mapId);

        MapDTO Insert(MapDTO map);

        #endregion
    }
}