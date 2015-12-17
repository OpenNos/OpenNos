using OpenNos.Data;
using System.Collections.Generic;

namespace  OpenNos.DAL.Interface
{
    public interface INpcDAO
    {
        IEnumerable<NpcDTO> LoadFromMap(short MapId);
    }
}