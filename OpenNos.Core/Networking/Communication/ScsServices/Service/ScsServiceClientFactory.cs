using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// This class is used to create service client objects that is used in server-side.
    /// </summary>
    public static class ScsServiceClientFactory
    {
        #region Methods

        /// <summary>
        /// Creates a new service client object that is used in server-side.
        /// </summary>
        /// <param name="serverClient">Underlying server client object</param>
        /// <param name="requestReplyMessenger">RequestReplyMessenger object to send/receive messages over serverClient</param>
        /// <returns></returns>
        public static IScsServiceClient CreateServiceClient(IScsServerClient serverClient, RequestReplyMessenger<IScsServerClient> requestReplyMessenger)
        {
            return new ScsServiceClient(serverClient, requestReplyMessenger);
        }

        #endregion
    }
}