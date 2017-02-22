using System.Collections.Generic;

namespace OpenNos.Data
{
    public class WorldserverGroupDTO
    {
        #region Instantiation

        public WorldserverGroupDTO(string groupName, WorldserverDTO firstWorldserver)
        {
            GroupName = groupName;
            Servers = new List<WorldserverDTO> { firstWorldserver };
        }

        #endregion

        #region Properties

        public string GroupName { get; set; }

        public List<WorldserverDTO> Servers { get; set; }

        #endregion
    }
}