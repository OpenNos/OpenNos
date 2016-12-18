using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using System;
using System.Collections.Generic;
using System.Net;

namespace OpenNos.Data
{
    public class WorldserverGroupDTO
    {
        #region Properties

        public WorldserverGroupDTO(string groupName, WorldserverDTO firstWorldserver)
        {
            GroupName = groupName;
            Servers = new List<WorldserverDTO>() { firstWorldserver };
        } 

        public List<WorldserverDTO> Servers { get; set; }

        public string GroupName { get; set; }

        #endregion
    }
}