using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messengers;
using System.Collections.Generic;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IScsClient : IMessenger, IConnectableClient
    {
        //Does not define any additional member
    }
}
