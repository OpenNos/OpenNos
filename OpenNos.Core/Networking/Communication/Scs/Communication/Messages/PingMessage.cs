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

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// This message is used to send/receive ping messages. Ping messages is used to keep connection
    /// alive between server and client.
    /// </summary>
    [Serializable]
    public sealed class ScsPingMessage : ScsMessage
    {
        #region Instantiation

        /// <summary>
        /// Creates a new PingMessage object.
        /// </summary>
        public ScsPingMessage()
        {
        }

        /// <summary>
        /// Creates a new reply PingMessage object.
        /// </summary>
        /// <param name="repliedMessageId">Replied message id if this is a reply for a message.</param>
        public ScsPingMessage(string repliedMessageId)
            : this()
        {
            RepliedMessageId = repliedMessageId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? $"ScsPingMessage [{MessageId}]"
                       : $"ScsPingMessage [{MessageId}] Replied To [{RepliedMessageId}]";
        }

        #endregion
    }
}