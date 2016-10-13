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

using System;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Channels
{
    /// <summary>
    /// Represents a communication listener. A connection listener is used to accept incoming client
    /// connection requests.
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