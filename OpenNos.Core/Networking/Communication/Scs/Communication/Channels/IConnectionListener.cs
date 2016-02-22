using System;

namespace OpenNos.Core.Communication.Scs.Communication.Channels
{
    /// <summary>
    /// Represents a communication listener.
    /// A connection listener is used to accept incoming client connection requests.
    /// </summary>
    public interface IConnectionListener
    {
        #region Events

        /// <summary>
        /// This event is raised when a new communication channel connected.
        /// </summary>
        event EventHandler<CommunicationChannelEventArgs> CommunicationChannelConnected;

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        void Stop();

        #endregion
    }
}