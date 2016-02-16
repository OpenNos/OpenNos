using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface INpcDAO
    {
        #region Methods

        IEnumerable<NpcDTO> LoadFromMap(short MapId);

        NpcDTO Insert(NpcDTO npc);

        #endregion
    }
}