using System.Collections.Generic;

namespace OpenNos.Master.Library
{
    public class WorldServerGroup
    {
        #region Instantiation

        public WorldServerGroup(string groupName, WorldServer firstWorldserver)
        {
            GroupName = groupName; 
            Servers = new List<WorldServer> { firstWorldserver };
        }

        #endregion

        #region Properties

        public string GroupName { get; set; }

        public List<WorldServer> Servers { get; set; }

        #endregion
    }
}