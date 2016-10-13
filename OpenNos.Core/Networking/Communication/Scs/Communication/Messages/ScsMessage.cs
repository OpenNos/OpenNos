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
    /// Represents a message that is sent and received by server and client. This is the base class
    /// for all messages.
    /// </summary>
    [Serializable]
    public class ScsMessage : IScsMessage
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ScsMessage.
        /// </summary>
        public ScsMessage()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new reply ScsMessage.
        /// </summary>
        /// <param name="repliedMessageId">Replied message id if this is a reply for a message.</param>
        public ScsMessage(string repliedMessageId)
            : this()
        {
            RepliedMessageId = repliedMessageId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Unique identified for this message. Default value: New GUID. Do not change if you do not
        /// want to do low level changes such as custom wire protocols.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// This property is used to indicate that this is a Reply message to a message. It may be
        /// null if this is not a reply message.
        /// </summary>
        public string RepliedMessageId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? $"ScsMessage [{MessageId}]"
                       : $"ScsMessage [{MessageId}] Replied To [{RepliedMessageId}]";
        }

        #endregion
    }
}