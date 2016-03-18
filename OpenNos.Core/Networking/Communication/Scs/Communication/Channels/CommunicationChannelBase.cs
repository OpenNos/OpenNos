using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Channels
{
    /// <summary>
    /// This class provides base functionality for all communication channel Classs.
    /// </summary>
    public abstract class CommunicationChannelBase : ICommunicationChannel
    {
        #region Instantiation

        /// <summary>
        /// Constructor.
        /// </summary>
        protected CommunicationChannelBase()
        {
            CommunicationState = CommunicationStates.Disconnected;
            LastReceivedMessageTime = DateTime.MinValue;
            LastSentMessageTime = DateTime.MinValue;
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when communication channel closed.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// This event is raised when a new message is received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error.
        /// It does not guaranties that message is properly handled and processed by remote application.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        public CommunicationStates CommunicationState { get; protected set; }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime { get; protected set; }

        /// <summary>
        /// Gets the time of the last succesfully sent message.
        /// </summary>
        public DateTime LastSentMessageTime { get; protected set; }

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        public abstract ScsEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets/sets wire protocol that the channel uses.
        /// This property must set before first communication.
        /// </summary>
        public IScsWireProtocol WireProtocol { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Disconnects from remote application and closes this channel.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if message is null</exception>
        public void SendMessage(IScsMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            SendMessagepublic(message);
        }

        /// <summary>
        /// Starts the communication with remote application.
        /// </summary>
        public void Start()
        {
            Startpublic();
            CommunicationState = CommunicationStates.Connected;
        }

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageReceived(IScsMessage message)
        {
            var handler = MessageReceived;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }

        /// <summary>
        /// Raises MessageSent event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageSent(IScsMessage message)
        {
            var handler = MessageSent;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// This method is overrided by derived Classs to really send to message.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected abstract void SendMessagepublic(IScsMessage message);

        /// <summary>
        /// Starts the communication with remote application really.
        /// </summary>
        protected abstract void Startpublic();

        #endregion
    }
}