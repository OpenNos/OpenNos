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
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;
using System.Threading.Tasks;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Messengers
{
    /// <summary>
    /// Represents an object that can send and receive messages.
    /// </summary>
    public interface IMessenger
    {
        #region Events

        /// <summary>
        /// This event is raised when a new message is received.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error. It does not guaranties
        /// that message is properly handled and processed by remote application.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        DateTime LastReceivedMessageTime { get; }

        /// <summary>
        /// Gets the time of the last succesfully sent message.
        /// </summary>
        DateTime LastSentMessageTime { get; }

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        IScsWireProtocol WireProtocol { get; set; }

        #endregion

        #region Methods

        Task ClearLowPriorityQueue();

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        void SendMessage(IScsMessage message, byte priority);

        #endregion
    }
}