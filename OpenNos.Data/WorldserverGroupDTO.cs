using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using System.Collections.Generic;
using System.Net;

namespace OpenNos.Data
{
    public class WorldserverGroupDTO
    {
        #region Properties

        public List<ScsTcpEndPoint> Addresses { get; set; }

        public string GroupName { get; set; }

        #endregion
    }
}