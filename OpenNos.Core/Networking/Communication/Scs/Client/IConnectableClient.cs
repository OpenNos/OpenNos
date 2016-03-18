using OpenNos.Core.Networking.Communication.Scs.Communication;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client for SCS servers.
    /// </summary>
    public interface IConnectableClient : IDisposable
    {
        #region Events

        /// <summary>
        /// This event is raised when client connected to server.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        event EventHandler Disconnected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        CommunicationStates CommunicationState { get; }

        /// <summary>
        /// Timeout for connecting to a server (as milliseconds).
        /// Default value: 15 seconds (15000 ms).
        /// </summary>
        int ConnectTimeout { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Connects to server.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects from server.
        /// Does nothing if already disconnected.
        /// </summary>
        void Disconnect();

        #endregion
    }
}