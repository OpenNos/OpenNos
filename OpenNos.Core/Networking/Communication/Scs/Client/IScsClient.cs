using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;

namespace OpenNos.Core.Networking.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IScsClient : IMessenger, IConnectableClient
    {
        //Does not define any additional member
    }
}