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

using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System.Collections.Generic;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Protocols
{
    /// <summary>
    /// Represents a byte-level communication protocol between applications.
    /// </summary>
    public interface IScsWireProtocol
    {
        #region Methods

        /// <summary>
        /// Builds messages from a byte array that is received from remote application. The Byte
        /// array may contain just a part of a message, the protocol must cumulate bytes to build
        /// messages. This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="receivedBytes">Received bytes from remote application</param>
        /// <returns>
        /// List of messages. Protocol can generate more than one message from a byte array. Also, if
        /// received bytes are not sufficient to build a message, the protocol may return an empty
        /// list (and save bytes to combine with next method call).
        /// </returns>
        IEnumerable<IScsMessage> CreateMessages(byte[] receivedBytes);

        /// <summary>
        /// Serializes a message to a byte array to send to remote application. This method is
        /// synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        byte[] GetBytes(IScsMessage message);

        /// <summary>
        /// This method is called when connection with remote application is reset (connection is
        /// renewing or first connecting). So, wire protocol must reset itself.
        /// </summary>
        void Reset();

        #endregion
    }
}