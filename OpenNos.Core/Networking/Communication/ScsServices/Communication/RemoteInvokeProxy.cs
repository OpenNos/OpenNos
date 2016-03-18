using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;
using OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace OpenNos.Core.Networking.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class is used to generate a dynamic proxy to invoke remote methods.
    /// It translates method invocations to messaging.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    public class RemoteInvokeProxy<TProxy, TMessenger> : RealProxy where TMessenger : IMessenger
    {
        #region Members

        /// <summary>
        /// Messenger object that is used to send/receive messages.
        /// </summary>
        private readonly RequestReplyMessenger<TMessenger> _clientMessenger;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new RemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        public RemoteInvokeProxy(RequestReplyMessenger<TMessenger> clientMessenger)
            : base(typeof(TProxy))
        {
            _clientMessenger = clientMessenger;
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
            var message = msg as IMethodCallMessage;
            if (message == null)
            {
                return null;
            }

            var requestMessage = new ScsRemoteInvokeMessage
            {
                ServiceClassName = typeof(TProxy).Name,
                MethodName = message.MethodName,
                Parameters = message.InArgs
            };

            var responseMessage = _clientMessenger.SendMessageAndWaitForResponse(requestMessage) as ScsRemoteInvokeReturnMessage;
            if (responseMessage == null)
            {
                return null;
            }

            return responseMessage.RemoteException != null
                       ? new ReturnMessage(responseMessage.RemoteException, message)
                       : new ReturnMessage(responseMessage.ReturnValue, null, 0, message.LogicalCallContext, message);
        }

        #endregion
    }
}