using OpenNos.Core.Communication.Scs.Client;
using OpenNos.Core.Communication.Scs.Communication;
using OpenNos.Core.Communication.Scs.Communication.Messengers;
using System.Runtime.Remoting.Messaging;

namespace OpenNos.Core.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class extends RemoteInvokeProxy to provide auto connect/disconnect mechanism
    /// if client is not connected to the server when a service method is called.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    public class AutoConnectRemoteInvokeProxy<TProxy, TMessenger> : RemoteInvokeProxy<TProxy, TMessenger> where TMessenger : IMessenger
    {
        #region Members

        /// <summary>
        /// Reference to the client object that is used to connect/disconnect.
        /// </summary>
        private readonly IConnectableClient _client;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new AutoConnectRemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        /// <param name="client">Reference to the client object that is used to connect/disconnect</param>
        public AutoConnectRemoteInvokeProxy(RequestReplyMessenger<TMessenger> clientMessenger, IConnectableClient client)
            : base(clientMessenger)
        {
            _client = client;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overrides message calls and translates them to messages to remote application.
        /// </summary>
        /// <param name="msg">Method invoke message (from RealProxy base class)</param>
        /// <returns>Method invoke return message (to RealProxy base class)</returns>
        public override IMessage Invoke(IMessage msg)
        {
            if (_client.CommunicationState == CommunicationStates.Connected)
            {
                //If already connected, behave as base class (RemoteInvokeProxy).
                return base.Invoke(msg);
            }

            //Connect, call method and finally disconnect
            _client.Connect();
            try
            {
                return base.Invoke(msg);
            }
            finally
            {
                _client.Disconnect();
            }
        }

        #endregion
    }
}