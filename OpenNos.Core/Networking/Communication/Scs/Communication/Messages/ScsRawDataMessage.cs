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
    /// This message is used to send/receive a raw byte array as message data.
    /// </summary>
    [Serializable]
    public class ScsRawDataMessage : ScsMessage, IComparable
    {
        #region Instantiation

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ScsRawDataMessage()
        {
        }

        /// <summary>
        /// Creates a new ScsRawDataMessage object with MessageData property.
        /// </summary>
        /// <param name="messageData">Message data that is being transmitted</param>
        public ScsRawDataMessage(byte[] messageData)
        {
            MessageData = messageData;
        }

        /// <summary>
        /// Creates a new reply ScsRawDataMessage object with MessageData property.
        /// </summary>
        /// <param name="messageData">Message data that is being transmitted</param>
        /// <param name="repliedMessageId">Replied message id if this is a reply for a message.</param>
        public ScsRawDataMessage(byte[] messageData, string repliedMessageId)
            : this(messageData)
        {
            RepliedMessageId = repliedMessageId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Message data that is being transmitted.
        /// </summary>
        public byte[] MessageData { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Methods

        public int CompareTo(object obj)
        {
            return CompareTo((ScsRawDataMessage)obj);
        }

        public int CompareTo(ScsRawDataMessage other)
        {
            return Priority.CompareTo(other.Priority);
        }

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            var messageLength = MessageData == null ? 0 : MessageData.Length;
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? $"ScsRawDataMessage [{MessageId}]: {messageLength} bytes"
                       : $"ScsRawDataMessage [{MessageId}] Replied To [{RepliedMessageId}]: {messageLength} bytes";
        }

        #endregion
    }
}