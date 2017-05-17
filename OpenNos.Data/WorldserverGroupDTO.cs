using System.Collections.Generic;

namespace OpenNos.Data
{
    public class WorldServerGroupDTO
    {
        #region Instantiation

        public WorldServerGroupDTO(string groupName, WorldServerDTO firstWorldserver)
        {
            GroupName = groupName;
            Servers = new List<WorldServerDTO> { firstWorldserver };
        }

        #endregion

        #region Properties

        public string GroupName { get; set; }

        public List<WorldServerDTO> Servers { get; set; }

        #endregion
    }
}