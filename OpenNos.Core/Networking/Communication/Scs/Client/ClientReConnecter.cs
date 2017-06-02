/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Threading;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Client
{
    /// <summary>
    /// This class is used to automatically re-connect to server if disconnected. It attempts to
    /// reconnect to server periodically until connection established.
    /// </summary>
    public class ClientReConnecter : IDisposable
    {
        #region Members

        /// <summary>
        /// Reference to client object.
        /// </summary>
        private readonly IConnectableClient _client;

        /// <summary>
        /// Timer to attempt ro reconnect periodically.
        /// </summary>
        private readonly Timer _reconnectTimer;

        /// <summary>
        /// Indicates the dispose state of this object.
        /// </summary>
        private volatile bool _disposed;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ClientReConnecter object. It is not needed to start ClientReConnecter since
        /// it automatically starts when the client disconnected.
        /// </summary>
        /// <param name="client">Reference to client object</param>
        /// <exception cref="ArgumentNullException">
        /// Throws ArgumentNullException if client is null.
        /// </exception>
        public ClientReConnecter(IConnectableClient client)
        {
            _client = client ?? throw new ArgumentNullException("client");
            _client.Disconnected += Client_Disconnected;
            _reconnectTimer = new Timer(20000);
            _reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            _reconnectTimer.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Reconnect check period.
        /// Default: 20 seconds.
        /// </summary>
        public int ReConnectCheckPeriod
        {
            get { return _reconnectTimer.Period; }
            set { _reconnectTimer.Period = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes this object. Does nothing if already disposed.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Disconnected -= Client_Disconnected;
                _reconnectTimer.Stop();
                _client.Dispose();
                _reconnectTimer.Dispose();
            }
        }

        /// <summary>
        /// Handles Disconnected event of _client object.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            _reconnectTimer.Start();
        }

        /// <summary>
        /// Hadles Elapsed event of _reconnectTimer.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments</param>
        private void ReconnectTimer_Elapsed(object sender, EventArgs e)
        {
            if (_disposed || _client.CommunicationState == CommunicationStates.Connected)
            {
                _reconnectTimer.Stop();
                return;
            }

            try
            {
                _client.Connect();
                _reconnectTimer.Stop();
            }
            catch
            {
                // No need to catch since it will try to re-connect again
            }
        }

        #endregion
    }
}