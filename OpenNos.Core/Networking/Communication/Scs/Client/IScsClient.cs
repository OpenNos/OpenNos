using OpenNos.Core.Communication.Scs.Communication;
using OpenNos.Core.Communication.Scs.Communication.Messengers;
using System.Collections.Generic;

namespace OpenNos.Core.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IScsClient : IMessenger, IConnectableClient
    {
        //Does not define any additional member
    }
}
